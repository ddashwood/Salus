using Microsoft.EntityFrameworkCore;
using Salus.Models.Changes;

namespace Salus.Saving;

internal interface IDbContextSaver<TKey>
{
    Save<TKey>? BuildPreliminarySave(DbContext context);
    Task<Save<TKey>?> BuildPreliminarySaveAsync(CancellationToken cancellationToken, DbContext context);

    void Apply(DbContext context, IEnumerable<ChangedRow> changes);
}
