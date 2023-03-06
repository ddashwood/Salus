namespace Salus.HostedServices;

internal interface IQueueProcessor<TContext, TKey> where TContext : SalusDbContext<TKey>
{
    Task ProcessQueue();
}
