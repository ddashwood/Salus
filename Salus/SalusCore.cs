using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Salus.Idempotency;
using Salus.Messaging;
using Salus.Models.Changes;
using Salus.Models.Entities;
using Salus.Saving;
using System.Diagnostics.CodeAnalysis;

namespace Salus;

/// <summary>
/// The core internal features of Salus.
/// </summary>
internal class SalusCore : ISalus, ISalusCore
{
    private readonly IDbContextIdempotencyChecker _idempotencyChecker;
    private readonly IDbContextSaver _saver;
    private readonly IMessageSenderInternal _messageSender;
    private readonly ILogger<SalusCore>? _logger;

    private ISalusDbContext? _salusContext;
    private DbContext? _dbContext;
    
    /// <inheritdoc/>
    public SalusOptions Options { get; }

    /// <summary>
    /// Constructs an instance of Salus core features.
    /// </summary>
    /// <param name="idempotencyChecker">An Idempotency checker.</param>
    /// <param name="saver">A Saver.</param>
    /// <param name="messageSender">A Message Sender.</param>
    /// <param name="options">The options to be used by this instance.</param>
    /// <param name="logger">A logger.</param>
    public SalusCore(
        IDbContextIdempotencyChecker idempotencyChecker,
        IDbContextSaver saver,
        IMessageSenderInternal messageSender,
        SalusOptions? options,
        ILogger<SalusCore>? logger)
    {
        ArgumentNullException.ThrowIfNull(idempotencyChecker);
        ArgumentNullException.ThrowIfNull(saver);
        ArgumentNullException.ThrowIfNull(messageSender);
        _idempotencyChecker = idempotencyChecker;
        _saver = saver;
        _messageSender = messageSender;
        Options = options ?? new SalusOptions();
        _logger = logger;
    }

    /// <inheritdoc/>
    public void Init<TContext>(TContext context) where TContext : DbContext, ISalusDbContext
    {
        ArgumentNullException.ThrowIfNull(context);
        _salusContext = context;
        _dbContext = context;
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
        modelBuilder.Entity<SalusSaveEntity>(e =>
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
    public void Apply(Save save)
    {
        CheckInitialised();
        _saver.Apply(_dbContext, save.Changes);
    }

    /// <inheritdoc/>
    public int SaveChanges(bool acceptAllChangesOnSuccess, Func<bool, int> baseSaveChanges)
    {
        CheckInitialised();

        var result = BuildPreliminarySave();

        IDbContextTransaction? tran = null;
        try
        {
            if (_dbContext.Database.CurrentTransaction == null)
            {
                // If we're not already in a transaction, we create one here.
                // If we *are* already in a transaction, that transaction will be sufficient

                tran = _dbContext.Database.BeginTransaction();
            }

            baseSaveChanges(acceptAllChangesOnSuccess);

            if (result != null)
            {
                CompleteSave(result);
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


        if (result == null)
        {
            return 0;
        }

        if (_dbContext.Database.CurrentTransaction == null)
        {
            SendMessages(result);
        }
        else
        {
            _salusContext.SalusDatabase.AddTransactionSave(result);
        }
        return result.Changes.Count;
    }

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken, Func<bool, CancellationToken, Task<int>> baseSaveChanges)
    {
        CheckInitialised();

        var result = await BuildPreliminarySaveAsync(cancellationToken).ConfigureAwait(false);

        IDbContextTransaction? tran = null;
        try
        {
            if (_dbContext.Database.CurrentTransaction == null)
            {
                // If we're not already in a transaction, we create one here.
                // If we *are* already in a transaction, that transaction will be sufficient

                tran = await _dbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            }

            await baseSaveChanges(acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(false);

            if (result != null)
            {
                await CompleteSaveAsync(result).ConfigureAwait(false);
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


        if (result == null)
        {
            return 0;
        }

        if (_dbContext.Database.CurrentTransaction == null)
        {
            await SendMessageAsync(result).ConfigureAwait(false);
        }
        else
        {
            _salusContext.SalusDatabase.AddTransactionSave(result);
        }
        return result.Changes.Count;
    }

    private Save? BuildPreliminarySave()
    {
        CheckInitialised();
        return _saver.BuildPreliminarySave(_dbContext);
    }

    private async Task<Save?> BuildPreliminarySaveAsync(CancellationToken cancellationToken)
    {
        CheckInitialised();
        return await _saver.BuildPreliminarySaveAsync(cancellationToken, _dbContext).ConfigureAwait(false);
    }

    private void CompleteSave(Save save)
    {
        CheckInitialised();
        CompleteSaveCommon(save);
        _dbContext.SaveChanges();
    }

    private async Task CompleteSaveAsync(Save save)
    {
        CheckInitialised();
        CompleteSaveCommon(save);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    private void CompleteSaveCommon(Save save)
    {
        CheckInitialised();

        foreach (var change in save.Changes)
        {
            change.CompleteAfterSave();
        }
        var entity = new SalusSaveEntity(save);
        // _dbContext and _salusContext point to the same context object
        _salusContext.SalusSaves.Add(entity);
    }

    /// <inheritdoc/>
    public void SendMessages(Save save)
    {
        CheckInitialised();
        var saveEntity = _salusContext.SalusSaves.SingleOrDefault(s => s.Id == save.Id);
        _messageSender.Send(JsonConvert.SerializeObject(save), saveEntity, _dbContext);
    }

    /// <inheritdoc/>
    public async Task SendMessageAsync(Save save)
    {
        CheckInitialised();
        var saveEntity = await _salusContext.SalusSaves.SingleOrDefaultAsync(s => s.Id == save.Id).ConfigureAwait(false);
        await _messageSender.SendAsync(JsonConvert.SerializeObject(save), saveEntity, _dbContext).ConfigureAwait(false);
    }
}
