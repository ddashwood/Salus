using Microsoft.EntityFrameworkCore;
using Salus;

namespace SalusExampleParent;

internal class ExampleDbContext : SalusDbContext
{
    public ExampleDbContext(ISalus salus) : base(salus)
    {
    }

    public ExampleDbContext(ISalus salus, DbContextOptions<ExampleDbContext> options) : base(salus, options)
    {
    }
}
