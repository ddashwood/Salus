using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Salus.HostedServices;

internal class QueueProcessorService<TContext> : IHostedService where TContext : SalusDbContext
{
    private readonly ILogger<QueueProcessorService<TContext>> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private Timer? _timer;

    public QueueProcessorService(ILogger<QueueProcessorService<TContext>> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Queue Processor starting");

            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
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
                var queueProcessor = scope.ServiceProvider.GetRequiredService<IQueueProcessor<TContext>>();
                await queueProcessor.ProcessQueue();
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Queue Processor encountered an unhandled exception");
            Environment.Exit(-1);
        }
    }
}
