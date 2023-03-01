using Microsoft.EntityFrameworkCore;
using Salus;
using SalusTests.TestDataStructures.Entities;

namespace SalusTests.TestDataStructures.Contexts;

internal class NonSalusEntityContext : SalusDbContext
{
    public NonSalusEntityContext(
        ISalusCore salus,
        DbContextOptions options
    )
        : base(salus, options)
    {
    }


    public DbSet<NoKeyAnnotationIntIdEntity> Ents => Set<NoKeyAnnotationIntIdEntity>();

    protected override void OnSalusModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NoKeyAnnotationIntIdEntity>()
            .Property(e => e.Id)
            .ValueGeneratedOnAdd();
    }
}
