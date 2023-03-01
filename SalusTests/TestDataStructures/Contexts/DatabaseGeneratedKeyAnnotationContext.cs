using Microsoft.EntityFrameworkCore;
using Salus;
using SalusTests.TestDataStructures.Entities;

namespace SalusTests.TestDataStructures.Contexts;

internal class DatabaseGeneratedKeyAnnotationContext : SalusDbContext
{
    public DatabaseGeneratedKeyAnnotationContext(
        ISalusCore salus,
        DbContextOptions options
    )
        : base(salus, options)
    {
    }

    [SalusDbSet]
    public DbSet<DatabaseGeneratedKeyAnnotationEntity> Ents => Set<DatabaseGeneratedKeyAnnotationEntity>();
}
