using Microsoft.EntityFrameworkCore;
using Salus.Idempotency;
using Salus.Models.Changes;
using Salus.Saving;

namespace Salus;

internal class SalusCore : ISalusCore
{
    private readonly IDbContextIdempotencyChecker _idempotencyChecker;
    private readonly IDbContextSaver _saver;
    public SalusCore(IDbContextIdempotencyChecker idempotencyChecker, IDbContextSaver saver)
        : base()
    {
        _idempotencyChecker = idempotencyChecker;
        _saver = saver;
    }

    public void Check(ModelBuilder modelBuilder, SalusDbContext context)
    {
        _idempotencyChecker.Check(modelBuilder, context);
    }



    public void Apply(DbContext context, IEnumerable<Change> changes)
    {
        _saver.Apply(context, changes);
    }

    public int SaveChanges<TContext>(TContext context) where TContext : DbContext, ISalusDbContext
    {
        return _saver.SaveChanges(context);
    }

    public async Task<int> SaveChangesAsync<TContext>(CancellationToken cancellationToken, TContext context) where TContext : DbContext, ISalusDbContext
    {
        return await _saver.SaveChangesAsync(cancellationToken, context);
    }
}
