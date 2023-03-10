namespace Salus.Purging;

internal interface IPurger<TContext, TKey> where TContext : SalusDbContext<TKey>
{
    Task PurgeAsync();
}
