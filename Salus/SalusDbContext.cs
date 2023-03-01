using Microsoft.EntityFrameworkCore;
using Salus.Models;
using Salus.Models.Changes;

namespace Salus;

public class SalusDbContext : DbContext, ISalusDbContext
{
    private readonly ISalusCore _salus;

    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("EF Core isn't fully compatible with trimming, and running the application may generate unexpected runtime failures. Some specific coding pattern are usually required to make trimming work properly, see https://aka.ms/efcore-docs-trimming for more details.")]
    protected SalusDbContext(ISalusCore salus)
        : base()
    {
        _salus = salus;
    }

    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("EF Core isn't fully compatible with trimming, and running the application may generate unexpected runtime failures. Some specific coding pattern are usually required to make trimming work properly, see https://aka.ms/efcore-docs-trimming for more details.")]
    protected SalusDbContext(ISalusCore salus, DbContextOptions options)
        : base(options)
    {
        _salus = salus;
    }

    public DbSet<SalusUpdateEntity> SalusDataChanges => Set<SalusUpdateEntity>();

    protected override sealed void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnSalusModelCreating(modelBuilder);
        _salus.Check(modelBuilder, this);
    }

    protected virtual void OnSalusModelCreating(ModelBuilder modelBuilder)
    {
    }

    public override int SaveChanges() => SaveChanges(acceptAllChangesOnSuccess: true);

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        var result = _salus.SaveChanges(this);
        base.SaveChanges(acceptAllChangesOnSuccess);
        return result;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await SaveChangesAsync(true, cancellationToken);

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        var result = await _salus.SaveChangesAsync(cancellationToken, this);
        if (acceptAllChangesOnSuccess)
        {
            ChangeTracker.AcceptAllChanges();
        }
        return result;
    }

    public void Apply(IEnumerable<Change> changes)
    {
        _salus.Apply(this, changes);
    }
}
