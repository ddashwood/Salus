using Microsoft.EntityFrameworkCore;
using Salus.Models.Changes;

namespace Salus;

public interface ISalusCore
{
    int SaveChanges<TContext>(TContext context) where TContext : DbContext, ISalusDbContext;
    Task<int> SaveChangesAsync<TContext>(CancellationToken cancellationToken, TContext context) where TContext : DbContext, ISalusDbContext;
    void Apply(DbContext context, IEnumerable<Change> changes);

    void Check(ModelBuilder modelBuilder, SalusDbContext context);


}
