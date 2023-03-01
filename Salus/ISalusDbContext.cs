using Microsoft.EntityFrameworkCore;
using Salus.Models;

namespace Salus;

internal interface ISalusDbContext
{
    DbSet<SalusUpdateEntity> SalusDataChanges { get; }

}
