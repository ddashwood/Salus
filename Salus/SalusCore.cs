using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Salus.Idempotency;
using Salus.Messaging;
using Salus.Models;
using Salus.Models.Changes;
using Salus.Saving;
using System.Diagnostics.CodeAnalysis;

namespace Salus;

internal class SalusCore : ISalus, ISalusCore
{
    private readonly IDbContextIdempotencyChecker _idempotencyChecker;
    private readonly IDbContextSaver _saver;
    private readonly IMessageSender _messageSender;
    private readonly ILogger<SalusCore>? _logger;

    private ISalusDbContext? _salusContext;
    private DbContext? _dbContext;
    
    public SalusOptions Options { get; }

    public SalusCore(
        IDbContextIdempotencyChecker idempotencyChecker,
        IDbContextSaver saver,
        IMessageSender messageSender,
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

    public void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SalusUpdateEntity>(e =>
        {
            e.HasIndex(u => new { u.CompletedDateTimeUtc, u.FailedMessageSendAttempts });
        });

        Check(modelBuilder);
    }

    private void Check(ModelBuilder modelBuilder)
    {
        CheckInitialised();
        _idempotencyChecker.Check(modelBuilder, _dbContext);
    }



    public void Apply(Save save)
    {
        CheckInitialised();
        _saver.Apply(_dbContext, save.Changes);
    }

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

    public async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken, Func<bool, CancellationToken, Task<int>> baseSaveChanges)
    {
        CheckInitialised();

        var result = await BuildPreliminarySaveAsync(cancellationToken);

        IDbContextTransaction? tran = null;
        try
        {
            if (_dbContext.Database.CurrentTransaction == null)
            {
                // If we're not already in a transaction, we create one here.
                // If we *are* already in a transaction, that transaction will be sufficient

                tran = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            }

            await baseSaveChanges(acceptAllChangesOnSuccess, cancellationToken);

            if (result != null)
            {
                await CompleteSaveAsync(result);
            }

            await (tran?.CommitAsync() ?? Task.CompletedTask);
        }
        catch
        {
            await (tran?.RollbackAsync() ?? Task.CompletedTask);
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
            await SendMessageAsync(result);
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
        return await _saver.BuildPreliminarySaveAsync(cancellationToken, _dbContext);
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
        await _dbContext.SaveChangesAsync();
    }

    private void CompleteSaveCommon(Save save)
    {
        CheckInitialised();

        foreach (var change in save.Changes)
        {
            change.CompleteAfterSave();
        }
        var entity = new SalusUpdateEntity(save);
        // _dbContext and _salusContext point to the same context object
        _salusContext.SalusDataChanges.Add(entity);
    }


    public void SendMessages(Save save)
    {
        CheckInitialised();

        try
        {
            _messageSender.Send(JsonConvert.SerializeObject(save));

            try
            {
                var saveEntity = _salusContext.SalusDataChanges.SingleOrDefault(s => s.Id == save.Id);
                if (saveEntity != null)
                {
                    saveEntity.CompletedDateTimeUtc = DateTime.UtcNow;
                    // _dbContext and _salusContext point to the same context object
                    _dbContext.SaveChanges();
                }
            }
            catch(Exception saveException)
            {
                _logger?.LogError(saveException, "Error saving Salus changes");
            }
        }
        catch(Exception ex)
        {
            _logger?.LogWarning(ex, "Error sending message - message will be queued to be re-tried later");

            try
            {
                var saveEntity = _salusContext.SalusDataChanges.SingleOrDefault(s => s.Id == save.Id);
                if (saveEntity != null)
                {
                    saveEntity.FailedMessageSendAttempts++;
                    saveEntity.LastFailedMessageSendAttemptUtc = DateTime.UtcNow;
                    saveEntity.NextMessageSendAttemptUtc = DateTime.UtcNow.AddSeconds(Math.Pow(5, saveEntity.FailedMessageSendAttempts)); // TO DO - Make this configurable
                    // _dbContext and _salusContext point to the same context object
                    _dbContext.SaveChanges();
                }
            }
            catch (Exception saveException)
            {
                _logger?.LogError(saveException, "Error recording message send failure");
            }
        }
    }

    public async Task SendMessageAsync(Save save)
    {
        CheckInitialised();

        try
        {
            await _messageSender.SendAsync(JsonConvert.SerializeObject(save));

            try
            {
                var saveEntity = await _salusContext.SalusDataChanges.SingleOrDefaultAsync(s => s.Id == save.Id);
                if (saveEntity != null)
                {
                    saveEntity.CompletedDateTimeUtc = DateTime.UtcNow;
                    // _dbContext and _salusContext point to the same context object
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception saveException)
            {
                _logger?.LogError(saveException, "Error saving Salus changes");
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error sending message - message will be queued to be re-tried later");

            try
            {
                var saveEntity = await _salusContext.SalusDataChanges.SingleOrDefaultAsync(s => s.Id == save.Id);
                if (saveEntity != null)
                {
                    saveEntity.FailedMessageSendAttempts++;
                    saveEntity.LastFailedMessageSendAttemptUtc = DateTime.UtcNow;
                    // _dbContext and _salusContext point to the same context object
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception saveException)
            {
                _logger?.LogError(saveException, "Error recording message send failure");
            }
        }
    }
}
