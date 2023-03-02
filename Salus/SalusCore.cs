using Microsoft.EntityFrameworkCore;
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

    public void Check(ModelBuilder modelBuilder)
    {
        CheckInitialised();
        _idempotencyChecker.Check(modelBuilder, _dbContext);
    }



    public void Apply(Save save)
    {
        CheckInitialised();
        _saver.Apply(_dbContext, save.Changes);
    }

    public Save? SaveChanges()
    {
        CheckInitialised();
        var result = _saver.SaveChanges(_dbContext);

        if (result == null)
        {
            return null;
        }

        _salusContext.SalusDataChanges.Add(new SalusUpdateEntity(result));
        return result;
    }

    public async Task<Save?> SaveChangesAsync(CancellationToken cancellationToken)
    {
        CheckInitialised();
        var result = await _saver.SaveChangesAsync(cancellationToken, _dbContext);

        if (result == null)
        {
            return null;
        }

        _salusContext.SalusDataChanges.Add(new SalusUpdateEntity(result));
        return result;
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
}
