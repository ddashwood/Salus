using Microsoft.EntityFrameworkCore;
using Salus;
using SalusTests.TestDataStructures.Entities;

namespace SalusTests.TestDataStructures.Contexts;

internal class DatabaseGeneratedKeyAnnotationContext : SalusDbContext
{
    public DatabaseGeneratedKeyAnnotationContext(
        ISalus<int> salus,
        DbContextOptions<DatabaseGeneratedKeyAnnotationContext> options
    )
        : base(salus, options)
    {
    }

    [SalusDbSet]
    public DbSet<DatabaseGeneratedKeyAnnotationEntity> Ents => Set<DatabaseGeneratedKeyAnnotationEntity>();
}
