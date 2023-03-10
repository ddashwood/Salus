using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Salus.QueueProcessing;

namespace Salus.Purging;

internal class PurgerService<TContext, TKey> : IHostedService where TContext : SalusDbContext<TKey>
{
    private readonly ILogger<PurgerService<TContext, TKey>> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SalusOptions<TKey> _options;
    private Timer? _timer;

    public PurgerService(ILogger<PurgerService<TContext, TKey>> logger, IServiceScopeFactory scopeFactory, SalusOptions<TKey> options)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _options = options;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Purger starting");
            _timer = new Timer(DoWorkAsync, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(_options.RunPurgeIntervalMilliseconds));
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Purger failed to start with an exception");
            Environment.Exit(-1);
        }
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Purger is stopping.");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    private async void DoWorkAsync(object? state)
    {
        try
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var purger = scope.ServiceProvider.GetRequiredService<IPurger<TContext, TKey>>();
                await purger.PurgeAsync().ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Purger encountered an unhandled exception");
        }
    }
}
