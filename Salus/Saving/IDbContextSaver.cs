using Microsoft.EntityFrameworkCore;
using Salus.Models.Changes;

namespace Salus.Saving;

public interface IDbContextSaver
{
    int SaveChanges<TContext>(TContext context) where TContext : DbContext, ISalusDbContext;
    Task<int> SaveChangesAsync<TContext>(CancellationToken cancellationToken, TContext context) where TContext : DbContext, ISalusDbContext;

    void Apply(DbContext context, IEnumerable<Change> changes);
}
