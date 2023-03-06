using Microsoft.EntityFrameworkCore;
using Salus;

namespace SalusExampleParent;

internal class ExampleDbContext : SalusDbContext<int>
{
    public ExampleDbContext(ISalus<int> salus) : base(salus)
    {
    }

    public ExampleDbContext(ISalus<int> salus, DbContextOptions<ExampleDbContext> options) : base(salus, options)
    {
    }

    [SalusDbSet]
    public DbSet<ExampleData> ExampleData => Set<ExampleData>();
}
