using Microsoft.EntityFrameworkCore;
using Salus.Models.Entities;

namespace Salus;

internal interface ISalusDbContext
{
    DbSet<SalusSaveEntity> SalusSaves { get; }

    SalusDatabaseFacade SalusDatabase { get; }
}
