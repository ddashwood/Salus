using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;
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
        salus.Init(this);
        _salus = salus;
    }

    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("EF Core isn't fully compatible with trimming, and running the application may generate unexpected runtime failures. Some specific coding pattern are usually required to make trimming work properly, see https://aka.ms/efcore-docs-trimming for more details.")]
    protected SalusDbContext(ISalusCore salus, DbContextOptions options)
        : base(options)
    {
        salus.Init(this);
        _salus = salus;
    }

    public DbSet<SalusUpdateEntity> SalusDataChanges => Set<SalusUpdateEntity>();

    protected override sealed void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnSalusModelCreating(modelBuilder);
        _salus.Check(modelBuilder);
    }

    protected virtual void OnSalusModelCreating(ModelBuilder modelBuilder)
    {
    }

    public override int SaveChanges() => SaveChanges(acceptAllChangesOnSuccess: true);

    
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        var result = _salus.SaveChanges();
        base.SaveChanges(acceptAllChangesOnSuccess);

        if (result == null)
        {
            return 0;
        }

        if (Database.CurrentTransaction == null)
        {
            _salus.SendMessages(result);
        }
        return result.Changes.Count;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await SaveChangesAsync(true, cancellationToken);

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        var result = await _salus.SaveChangesAsync(cancellationToken);
        await base.SaveChangesAsync(acceptAllChangesOnSuccess);

        if (result == null)
        {
            return 0;
        }

        if (Database.CurrentTransaction == null)
        {
            _salus.SendMessages(result);
        }
        return result.Changes.Count;
    }

    public void Apply(Save save)
    {
        _salus.Apply(save);
    }
}
