using Microsoft.EntityFrameworkCore;

namespace Salus.Idempotency;

internal interface IDbContextIdempotencyChecker
{
    void Check(ModelBuilder modelBuilder, DbContext context);
}
