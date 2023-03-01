using Microsoft.EntityFrameworkCore;
using Salus.Models.Changes;

namespace Salus;

public interface ISalusCore
{
    void Init<TContext>(TContext context) where TContext : DbContext, ISalusDbContext;

    Save? SaveChanges();
    Task<Save?> SaveChangesAsync(CancellationToken cancellationToken);
    void Apply(Save save);

    void Check(ModelBuilder modelBuilder);
    void SendMessages(Save save);
}
