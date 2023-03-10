using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Salus.Idempotency;
using Salus.Messaging;
using Salus.Models.Changes;
using Salus.Models.Entities;
using Salus.QueueProcessing;
using Salus.Saving;
using Salus.Services;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Salus;

/// <summary>
/// The core internal features of Salus.
/// </summary>
internal class SalusCore<TKey> : ISalus<TKey>, ISalusCore<TKey>
{
    private readonly IDbContextIdempotencyChecker _idempotencyChecker;
    private readonly IDbContextSaver<TKey> _saver;
    private readonly IMessageSenderInternal<TKey> _messageSender;
    private readonly ILogger<SalusCore<TKey>> _logger;
    private readonly IQueueProcessorSemaphore _semaphore;
    private readonly ISalusDbContextProvider _databaseProvider;

    private ISalusDbContext<TKey>? _salusContext;
    private DbContext? _dbContext;

    /// <inheritdoc/>
    public SalusOptions<TKey> Options { get; }

    /// <summary>
    /// Constructs an instance of Salus core features.
    /// </summary>
    /// <param name="idempotencyChecker">An Idempotency checker.</param>
    /// <param name="saver">A Saver.</param>
    /// <param name="messageSender">A Message Sender.</param>
    /// <param name="options">The options to be used by this instance.</param>
    /// <param name="logger">A logger.</param>
    /// <param name="semaphore">A semaphore for accessing the message processing queue</param>
    public SalusCore(
        IDbContextIdempotencyChecker idempotencyChecker,
        IDbContextSaver<TKey> saver,
        IMessageSenderInternal<TKey> messageSender,
        SalusOptions<TKey>? options,
        ILogger<SalusCore<TKey>> logger,
        IQueueProcessorSemaphore semaphore,
        ISalusDbContextProvider databaseProvider)
    {
        ArgumentNullException.ThrowIfNull(idempotencyChecker);
        ArgumentNullException.ThrowIfNull(saver);
        ArgumentNullException.ThrowIfNull(messageSender);
        ArgumentNullException.ThrowIfNull(semaphore);

        ValidateGenericType();

        _idempotencyChecker = idempotencyChecker;
        _saver = saver;
        _messageSender = messageSender;
        Options = options ?? new SalusOptions<TKey>(null);
        _logger = logger;
        _semaphore = semaphore;
        _databaseProvider = databaseProvider;
    }

    private void ValidateGenericType()
    {
        var validTypes = new List<Type>
        {
            typeof(int),
            typeof(long),
            typeof(string),
            typeof(Guid)
        };

        if (!validTypes.Contains(typeof(TKey)))
        {
            throw new InvalidOperationException("Salus only works with key types of int, long, string and Guid");
        }
    }

    /// <inheritdoc/>
    public void Init<TContext>(TContext context) where TContext : DbContext, ISalusDbContext<TKey>
    {
        _logger.LogDebug("Initialising a new Salus instance - checking for valid context");
        ArgumentNullException.ThrowIfNull(context);
        _salusContext = context;
        _dbContext = context;
        _logger.LogDebug("Context appears valid");
    }

    [MemberNotNull(nameof(_salusContext), nameof(_dbContext))]
    private void CheckInitialised()
    {
        _ = _dbContext ?? throw new InvalidOperationException("Salus Core is not initialised");
        _ = _salusContext ?? throw new InvalidOperationException("Salus Core is not initialised");
    }

    /// <inheritdoc/>
    public void OnModelCreating(ModelBuilder modelBuilder)
    {
        _logger.LogDebug("On model creating");
        modelBuilder.Entity<SalusSaveEntity<TKey>>(e =>
        {
            e.HasIndex(u => new { u.CompletedDateTimeUtc, u.NextMessageSendAttemptUtc });
        });

        Check(modelBuilder);
    }

    private void Check(ModelBuilder modelBuilder)
    {
        CheckInitialised();
        _idempotencyChecker.Check(modelBuilder, _dbContext);
    }


    /// <inheritdoc/>
    public void Apply(Save<TKey> save)
    {
        _logger.LogDebug("Applying", save.Id);
        CheckInitialised();
        _saver.Apply(_dbContext, save.Changes);
    }

    /// <inheritdoc/>
    public int SaveChanges(bool acceptAllChangesOnSuccess, Func<bool, int> baseSaveChanges)
    {
        _logger.LogDebug("Save changes");
        CheckInitialised();

        var save = BuildPreliminarySave();
        int result;

        IDbContextTransaction? tran = null;
        try
        {
            if (_dbContext.Database.CurrentTransaction == null)
            {
                // If we're not already in a transaction, we create one here.
                // If we *are* already in a transaction, that transaction will be sufficient

                tran = _dbContext.Database.BeginTransaction();
            }

            result = baseSaveChanges(acceptAllChangesOnSuccess);

            if (save != null)
            {
                CompleteSave(save);
            }

            tran?.Commit();
        }
        catch
        {
            tran?.Rollback();
            throw;
        }
        finally
        {
            tran?.Dispose();
        }


        if (save == null)
        {
            return result;
        }

        if (_dbContext.Database.CurrentTransaction == null)
        {
            SendMessage(save);
        }
        else
        {
            _salusContext.SalusDatabase.AddTransactionSave(save);
        }
        return result;
    }

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken, Func<bool, CancellationToken, Task<int>> baseSaveChanges)
    {
        _logger.LogDebug("Save changes async");
        CheckInitialised();

        var save = await BuildPreliminarySaveAsync(cancellationToken).ConfigureAwait(false);
        int result;

        IDbContextTransaction? tran = null;
        try
        {
            if (_dbContext.Database.CurrentTransaction == null)
            {
                // If we're not already in a transaction, we create one here.
                // If we *are* already in a transaction, that transaction will be sufficient

                tran = await _dbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            }

            result = await baseSaveChanges(acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(false);

            if (save != null)
            {
                await CompleteSaveAsync(save).ConfigureAwait(false);
            }

            await (tran?.CommitAsync() ?? Task.CompletedTask).ConfigureAwait(false);
        }
        catch
        {
            await (tran?.RollbackAsync() ?? Task.CompletedTask).ConfigureAwait(false);
            throw;
        }
        finally
        {
            tran?.Dispose();
        }


        if (save == null)
        {
            return result;
        }

        if (_dbContext.Database.CurrentTransaction == null)
        {
            await SendMessageAsync(save).ConfigureAwait(false);
        }
        else
        {
            _salusContext.SalusDatabase.AddTransactionSave(save);
        }
        return result;
    }

    private Save<TKey>? BuildPreliminarySave()
    {
        CheckInitialised();
        return _saver.BuildPreliminarySave(_dbContext);
    }

    private async Task<Save<TKey>?> BuildPreliminarySaveAsync(CancellationToken cancellationToken)
    {
        CheckInitialised();
        return await _saver.BuildPreliminarySaveAsync(cancellationToken, _dbContext).ConfigureAwait(false);
    }

    private void CompleteSave(Save<TKey> save)
    {
        CheckInitialised();
        var entity = CompleteSaveCommon(save);
        _dbContext.SaveChanges();
        save.Id = entity.Id;
    }

    private async Task CompleteSaveAsync(Save<TKey> save)
    {
        CheckInitialised();
        var entity = CompleteSaveCommon(save);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        save.Id = entity.Id;
    }

    private SalusSaveEntity<TKey> CompleteSaveCommon(Save<TKey> save)
    {
        CheckInitialised();

        foreach (var change in save.Changes)
        {
            change.CompleteAfterSave();
        }
        var entity = new SalusSaveEntity<TKey>(save);
        // _dbContext and _salusContext point to the same context object
        _salusContext.SalusSaves.Add(entity);
        return entity;
    }

    /// <inheritdoc/>
    public void SendMessage(Save<TKey> save)
    {
        _logger.LogDebug("Send message", save.Id);
        CheckInitialised();
        // Only send the message if there is nothing in the queue, and the queue is not being processed
        // Otherwise, if we send it now it may get sent out of order - we can just leave it in the queue for now
        bool sendingNow = false;
        Task? task = null;

        if (_semaphore.Start())
        {
            try
            {
                if (!_salusContext.SalusSaves.Any(c => c.CompletedDateTimeUtc == null
                                                && DateTime.UtcNow >= c.NextMessageSendAttemptUtc))
                {
                    sendingNow = true;

                    // Fire+forget the message sending. Because Entity Framework is not multi-threaded,
                    // we need to create a new connection

                    var context = _databaseProvider.GetDatabase(_dbContext.GetType(), out var scope);
                    var salusContext = (ISalusDbContext<TKey>)context;

                    task = Task.Run(async () =>
                    {
                        SalusSaveEntity<TKey>? saveEntity = null;
                        try
                        {
                            saveEntity = await (salusContext?.SalusSaves?.SingleOrDefaultAsync(GetEqualsExpression(save.Id)) 
                                        ?? Task.FromResult<SalusSaveEntity<TKey>?>(null))
                                        .ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogError(ex, "Error getting the entity ready for sending the message");
                        }
                        await _messageSender.SendAsync(JsonConvert.SerializeObject(save), saveEntity, context);
                    }).ContinueWith(_ =>  scope?.Dispose());
                }
            }
            finally
            {
                _semaphore.Stop();
            }
        }

        if (!sendingNow)
        {
            // If we can't send the message now, record in the database that it is ready to be sent straight away
            var saveEntity = _salusContext.SalusSaves.SingleOrDefault(GetEqualsExpression(save.Id));
            if (saveEntity != null)
            {
                saveEntity.NextMessageSendAttemptUtc = DateTime.UtcNow;
                _dbContext.SaveChanges();
            }
        }

        if (Options.DoNotFireAndForget && task != null)
        {
            task.Wait();
        }
    }

    /// <inheritdoc/>
    public async Task SendMessageAsync(Save<TKey> save)
    {
        _logger.LogDebug("Send message async", save.Id);
        CheckInitialised();
        // Only send the message if there is nothing in the queue, and the queue is not being processed
        // Otherwise, if we send it now it may get sent out of order - we can just leave it in the queue for now
        bool sendingNow = false;
        Task? task = null;

        if (_semaphore.Start())
        {
            try
            {
                if (!await _salusContext.SalusSaves.AnyAsync(c => c.CompletedDateTimeUtc == null
                                                && DateTime.UtcNow >= c.NextMessageSendAttemptUtc).ConfigureAwait(false))
                {
                    sendingNow = true;

                    // Fire+forget the message sending. Because Entity Framework is not multi-threaded,
                    // we need to create a new connection

                    var context = _databaseProvider.GetDatabase(_dbContext.GetType(), out var scope);
                    var salusContext = (ISalusDbContext<TKey>)context;

                    task = Task.Run(async () =>
                    {
                        SalusSaveEntity<TKey>? saveEntity = null;
                        try
                        {
                            saveEntity = await (salusContext?.SalusSaves?.SingleOrDefaultAsync(GetEqualsExpression(save.Id))
                                        ?? Task.FromResult<SalusSaveEntity<TKey>?>(null))
                                        .ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogError(ex, "Error getting the entity ready for sending the message");
                        }
                        await _messageSender.SendAsync(JsonConvert.SerializeObject(save), saveEntity, context);
                    }).ContinueWith(_ => scope?.Dispose());
                }
            }
            finally
            {
                _semaphore.Stop();
            }
        }

        if (!sendingNow)
        {
            // If we can't send the message now, record in the database that it is ready to be sent straight away
            var saveEntity = await _salusContext.SalusSaves.SingleOrDefaultAsync(GetEqualsExpression(save.Id));
            if (saveEntity != null)
            {
                saveEntity.NextMessageSendAttemptUtc = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
            }
        }

        if (Options.DoNotFireAndForget && task != null)
        {
            task.Wait();
        }
    }

    private Expression<Func<SalusSaveEntity<TKey>, bool>> GetEqualsExpression(TKey id)
    {
        // We want to do this:
        //   SalusSaves.SingleOrDefault(s => s.Id == save.Id);
        // But == is not supported on generic types unless we restrict them to be a class, which we can't do as we need
        // to support integer data types. And Entity Framework does not know how to use IEqualityComparer. So this is
        // the workaround

        var param = Expression.Parameter(typeof(SalusSaveEntity<TKey>));
        var left = Expression.Property(param, "Id");
        var right = Expression.Constant(id);
        var equal = Expression.Equal(left, right);

        return (Expression<Func<SalusSaveEntity<TKey>, bool>>)Expression.Lambda(equal, param);
    }
}
