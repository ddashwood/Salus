using Microsoft.EntityFrameworkCore;
using Salus.Models.Changes;

namespace Salus;

public interface ISalusCore
{
    void Init<TContext>(TContext context) where TContext : DbContext, ISalusDbContext;

    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    void Apply(Save save);

    void Check(ModelBuilder modelBuilder);


}
