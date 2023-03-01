using Microsoft.EntityFrameworkCore;

namespace Salus.Idempotency;

public interface IDbContextIdempotencyChecker
{
    void Check(ModelBuilder modelBuilder, DbContext context);
}
