using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Salus.QueueProcessing;

internal class QueueProcessorService<TContext, TKey> : IHostedService where TContext : SalusDbContext<TKey>
{
    private readonly ILogger<QueueProcessorService<TContext, TKey>> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SalusOptions<TKey> _options;
    private Timer? _timer;

    public QueueProcessorService(ILogger<QueueProcessorService<TContext, TKey>> logger, IServiceScopeFactory scopeFactory, SalusOptions<TKey> options)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _options = options;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Queue Processor starting");
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(_options.RetryQueueProcessIntervalMilliseconds));
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Queue Processor failed to start with an exception");
            Environment.Exit(-1);
        }
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Queue Processor is stopping.");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    private async void DoWork(object? state)
    {
        try
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var queueProcessor = scope.ServiceProvider.GetRequiredService<IQueueProcessor<TContext, TKey>>();
                await queueProcessor.ProcessQueue().ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Queue Processor encountered an unhandled exception");
            Environment.Exit(-1);
        }
    }
}
