using Microsoft.EntityFrameworkCore;
using Salus.Models.Entities;

namespace Salus;

/// <summary>
/// Represents the internal functionality of a Salus DbContext
/// </summary>
internal interface ISalusDbContext
{
    /// <summary>
    /// A list of <see cref="SalusSaveEntity"/> objects that are yet to be commited to the database
    /// </summary>
    DbSet<SalusSaveEntity> SalusSaves { get; }

    /// <summary>
    /// Provides access to database related information and operations for this context.
    /// </summary>
    SalusDatabaseFacade SalusDatabase { get; }
}
