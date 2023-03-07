using Microsoft.EntityFrameworkCore;
using Salus.Models.Entities;

namespace Salus;

/// <summary>
/// Represents the internal functionality of a Salus DbContext
/// </summary>
internal interface ISalusDbContext<TKey>
{
    /// <summary>
    /// Records pertaining to the data which has been saved in this DbContext
    /// </summary>
    DbSet<SalusSaveEntity<TKey>> SalusSaves { get; }

    /// <summary>
    /// Provides access to database related information and operations for this context.
    /// </summary>
    SalusDatabaseFacade<TKey> SalusDatabase { get; }
}
