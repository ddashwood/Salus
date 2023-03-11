using Microsoft.EntityFrameworkCore;
using Salus;

namespace SalusExampleParent;

public class ExampleDbContext : SalusDbContext
{
    public ExampleDbContext(ISalus<int> salus, DbContextOptions<ExampleDbContext> options) : base(salus, options)
    {
    }

    [SalusSourceDbSet]
    public DbSet<ExampleData> ExampleData => Set<ExampleData>();
}
