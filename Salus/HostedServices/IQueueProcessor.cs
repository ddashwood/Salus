namespace Salus.HostedServices;

internal interface IQueueProcessor<TContext> where TContext : SalusDbContext
{
    Task ProcessQueue();
}
