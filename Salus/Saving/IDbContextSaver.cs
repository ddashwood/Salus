using Microsoft.EntityFrameworkCore;
using Salus.Models.Changes;

namespace Salus.Saving;

internal interface IDbContextSaver
{
    Save? BuildPreliminarySave(DbContext context);
    Task<Save?> BuildPreliminarySaveAsync(CancellationToken cancellationToken, DbContext context);

    void Apply(DbContext context, IEnumerable<Change> changes);
}
