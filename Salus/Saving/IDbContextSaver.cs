using Microsoft.EntityFrameworkCore;
using Salus.Models.Changes;

namespace Salus.Saving;

internal interface IDbContextSaver
{
    Save? SaveChanges(DbContext context);
    Task<Save> SaveChangesAsync(CancellationToken cancellationToken, DbContext context);

    void Apply(DbContext context, IEnumerable<Change> changes);
}
