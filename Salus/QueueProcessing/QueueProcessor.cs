using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Salus.Messaging;

namespace Salus.QueueProcessing;

internal class QueueProcessor<TContext, TKey> : IQueueProcessor<TContext, TKey> where TContext : SalusDbContext<TKey>
{
    private readonly TContext _context;
    private readonly ILogger<QueueProcessor<TContext, TKey>> _logger;
    private readonly IMessageSenderInternal<TKey> _messageSender;
    private readonly IQueueProcessorSemaphore _semaphore;

    public QueueProcessor(TContext context, ILogger<QueueProcessor<TContext, TKey>> logger, IMessageSenderInternal<TKey> messageSender, IQueueProcessorSemaphore semaphore)
    {
        _context = context;
        _logger = logger;
        _messageSender = messageSender;
        _semaphore = semaphore;
    }

    public async Task ProcessQueue()
    {
        if (_semaphore.Start())
        {
            try
            {
                _logger.LogInformation("Processing queue");

                var queue = await _context.SalusSaves
                    .Where(c => c.CompletedDateTimeUtc == null
                             && DateTime.UtcNow >= c.NextMessageSendAttemptUtc)
                    .OrderBy(c => c.Id)
                    .ToListAsync().ConfigureAwait(false);

                _logger.LogInformation($"Queue contains {queue.Count} items");

                foreach (var dataChange in queue)
                {
                    try
                    {
                        await _messageSender.SendAsync(dataChange.SaveJson, dataChange, _context).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error processing queue item");
                    }
                }
            }
            finally
            {
                _semaphore.Stop();
            }
        }
    }
}
