using Microsoft.EntityFrameworkCore;
using Salus;
using SalusTests.TestDataStructures.Entities;

namespace SalusTests.TestDataStructures.Contexts;

internal class DatabaseGeneratedKeyFluentContext : SalusDbContext
{
    public DatabaseGeneratedKeyFluentContext(
        ISalus<int> salus,
        DbContextOptions<DatabaseGeneratedKeyFluentContext> options
    )
        : base(salus, options)
    {
    }


    [SalusDbSet]
    public DbSet<NoKeyAnnotationIntIdEntity> Ents => Set<NoKeyAnnotationIntIdEntity>();

    protected override void OnSalusModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NoKeyAnnotationIntIdEntity>()
            .Property(e => e.Id)
            .ValueGeneratedOnAdd();
    }
}
