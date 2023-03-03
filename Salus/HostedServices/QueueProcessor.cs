using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Salus.HostedServices;

internal class QueueProcessor<TContext> : IQueueProcessor<TContext> where TContext : SalusDbContext
{
    private readonly TContext _context;
    private readonly ILogger<QueueProcessor<TContext>> _logger;

    public QueueProcessor(TContext context, ILogger<QueueProcessor<TContext>> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task ProcessQueue()
    {
        _logger.LogInformation("Processing queue");

        var queue = await  _context.SalusDataChanges
            .Where(c => c.CompletedDateTimeUtc == null
                     && c.FailedMessageSendAttempts < 10    // TO DO - Make this configurable
                     && DateTime.UtcNow >= c.NextMessageSendAttemptUtc)
            .ToListAsync();


    }
}
