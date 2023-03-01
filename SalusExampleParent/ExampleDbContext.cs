using Microsoft.EntityFrameworkCore;
using Salus;

namespace SalusExampleParent;

internal class ExampleDbContext : SalusDbContext
{
    public ExampleDbContext(ISalusCore salus) : base(salus)
    {
    }

    public ExampleDbContext(ISalusCore salus, DbContextOptions<ExampleDbContext> options) : base(salus, options)
    {
    }
}
