using Microsoft.Extensions.Logging;

namespace Salus.Purging;

internal class Purger<TContext, TKey> : IPurger<TContext, TKey> where TContext : SalusDbContext<TKey>
{
    private readonly TContext _context;
    private readonly ILogger<Purger<TContext, TKey>> _logger;
    private readonly IPurgerSemaphore _semaphore;
    private readonly SalusOptions<TKey> _options;

    public Purger(TContext context, ILogger<Purger<TContext, TKey>> logger, IPurgerSemaphore semaphore, SalusOptions<TKey> options)
    {
        _context = context;
        _logger = logger;
        _semaphore = semaphore;
        _options = options;
    }

    public async Task PurgeAsync()
    {
        if (_options.PurgeAfterSeconds == null)
        {
            _logger.LogDebug("Not purging - no time has been set");
            return;
        }

        if (_semaphore.Start())
        {
            try
            {
                _logger.LogDebug("Purging");


                var cutoffTime = DateTime.UtcNow.AddSeconds(_options.PurgeAfterSeconds.Value * -1);
                var entities = _context.SalusSaves.Where(s => s.CompletedDateTimeUtc < cutoffTime);
                _context.SalusSaves.RemoveRange(entities);
                var count = await _context.SaveChangesAsync();

                _logger.LogInformation($"Purged {count} records");
            }
            finally
            {
                _semaphore.Stop();
            }
        }
        else
        {
            _logger.LogDebug("Skipping purging because failed to obtain semaphore");
        }
    }
}
