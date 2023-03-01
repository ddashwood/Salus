using Microsoft.EntityFrameworkCore;
using Salus.Models;
using Salus.Models.Changes;

namespace Salus;

public interface ISalusDbContext
{
    DbSet<SalusUpdateEntity> SalusDataChanges { get; }

    void Apply(Save save);
}
