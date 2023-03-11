using Microsoft.EntityFrameworkCore;
using Salus;

namespace SalusExampleChild;

public class ChildContext : SalusDbContext
{
    public ChildContext(ISalus<int> salus, DbContextOptions<ChildContext> options) : base(salus, options)
    {
    }

    [SalusDestinationDbSet]
    public DbSet<ExampleData> ExampleData => Set<ExampleData>();

    protected override void OnSalusModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExampleData>()
            .Property(e => e.Id)
            .ValueGeneratedNever();
    }
}
