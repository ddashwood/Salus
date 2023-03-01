using Microsoft.EntityFrameworkCore;
using Salus;
using SalusTests.TestDataStructures.Entities;

namespace SalusTests.TestDataStructures.Contexts;

internal class DatabaseGeneratedKeyImplicitContext : SalusDbContext
{
    public DatabaseGeneratedKeyImplicitContext(
        ISalusCore salus,
        DbContextOptions<DatabaseGeneratedKeyImplicitContext> options
    )
        : base(salus, options)
    {
    }


    [SalusDbSet]
    public DbSet<NoKeyAnnotationIntIdEntity> Ents => Set<NoKeyAnnotationIntIdEntity>();
}
