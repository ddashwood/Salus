using Microsoft.EntityFrameworkCore;
using Salus;

namespace SalusExampleParent;

internal class ExampleDbContext : SalusDbContext
{
    public ExampleDbContext(ISalus<int> salus, DbContextOptions<ExampleDbContext> options) : base(salus, options)
    {
    }

    [SalusDbSet]
    public DbSet<ExampleData> ExampleData => Set<ExampleData>();
}
