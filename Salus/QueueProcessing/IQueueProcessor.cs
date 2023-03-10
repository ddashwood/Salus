namespace Salus.QueueProcessing;

internal interface IQueueProcessor<TContext, TKey> where TContext : SalusDbContext<TKey>
{
    Task ProcessQueueAsync();
}
