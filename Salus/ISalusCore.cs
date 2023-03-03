using Microsoft.EntityFrameworkCore;
using Salus.Models.Changes;

namespace Salus;

internal interface ISalusCore
{
    void Init<TContext>(TContext context) where TContext : DbContext, ISalusDbContext;

    int SaveChanges(bool acceptAllChangesOnSuccess, Func<bool, int> baseSaveChanges);

    Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, Func<bool, CancellationToken, Task<int>> baseSaveChanges);

    void Apply(Save save);

    void Check(ModelBuilder modelBuilder);
    
    void SendMessages(Save save);

    SalusOptions Options { get; }
}
