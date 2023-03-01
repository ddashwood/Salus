using Microsoft.EntityFrameworkCore;
using Salus.Idempotency;
using Salus.Models;
using Salus.Models.Changes;
using Salus.Saving;
using System.Diagnostics.CodeAnalysis;

namespace Salus;

internal class SalusCore : ISalusCore
{
    private readonly IDbContextIdempotencyChecker _idempotencyChecker;
    private readonly IDbContextSaver _saver;
    private ISalusDbContext? _salusContext;
    private DbContext? _dbContext;

    public SalusCore(IDbContextIdempotencyChecker idempotencyChecker, IDbContextSaver saver)
        : base()
    {
        ArgumentNullException.ThrowIfNull(idempotencyChecker);
        ArgumentNullException.ThrowIfNull(saver);
        _idempotencyChecker = idempotencyChecker;
        _saver = saver;
    }

    public void Init<TContext>(TContext context) where TContext : DbContext, ISalusDbContext
    {
        ArgumentNullException.ThrowIfNull(context);
        _salusContext = context;
        _dbContext = context;
    }

    [MemberNotNull(nameof(_salusContext), nameof(_dbContext))]
    private void CheckInitialised()
    {
        var _1 = _dbContext ?? throw new InvalidOperationException("Salus Core is not initialised");
        var _2 = _salusContext ?? throw new InvalidOperationException("Salus Core is not initialised");
    }

    public void Check(ModelBuilder modelBuilder)
    {
        CheckInitialised();
        _idempotencyChecker.Check(modelBuilder, _dbContext);
    }



    public void Apply(Save save)
    {
        CheckInitialised();
        _saver.Apply(_dbContext, save.Changes);
    }

    public int SaveChanges()
    {
        CheckInitialised();
        var result = _saver.SaveChanges(_dbContext);

        if (result == null)
        {
            return 0;
        }

        _salusContext.SalusDataChanges.Add(new SalusUpdateEntity(result));
        return result.Changes.Count;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        CheckInitialised();
        var result = await _saver.SaveChangesAsync(cancellationToken, _dbContext);
        _salusContext.SalusDataChanges.Add(new SalusUpdateEntity(result));
        return result.Changes.Count;
    }
}
