using Microsoft.EntityFrameworkCore;
using Salus;
using SalusTests.TestDataStructures.Entities;

namespace SalusTests.TestDataStructures.Contexts;

internal class NonGeneratedKeyContext : SalusDbContext
{
    public NonGeneratedKeyContext(
        ISalusCore salus,
        DbContextOptions<NonGeneratedKeyContext> options
    )
        : base(salus, options)
    {
    }


    [SalusDbSet]
    public DbSet<NoKeyAnnotationStringIdEntity> Ents => Set<NoKeyAnnotationStringIdEntity>();
}
