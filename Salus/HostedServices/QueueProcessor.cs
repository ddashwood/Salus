using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Salus.Messaging;
using Salus.Models.Changes;

namespace Salus.HostedServices;

internal class QueueProcessor<TContext> : IQueueProcessor<TContext> where TContext : SalusDbContext
{
    private readonly TContext _context;
    private readonly ILogger<QueueProcessor<TContext>> _logger;
    private readonly IMessageSender _messageSender;

    public QueueProcessor(TContext context, ILogger<QueueProcessor<TContext>> logger, IMessageSender messageSender)
    {
        _context = context;
        _logger = logger;
        _messageSender = messageSender;
    }

    public async Task ProcessQueue()
    {
        _logger.LogInformation("Processing queue");

        var queue = await  _context.SalusDataChanges
            .Where(c => c.CompletedDateTimeUtc == null
                     && DateTime.UtcNow >= c.NextMessageSendAttemptUtc)
            .ToListAsync().ConfigureAwait(false);

        _logger.LogInformation($"Queue contains {queue.Count} items");

        foreach (var dataChange in queue)
        {
            try
            {
                await _messageSender.SendAsync(dataChange.UpdateJson, dataChange, _context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error processing queue item");
            }
        }
    }
}
