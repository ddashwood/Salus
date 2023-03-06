using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Salus.Models.Changes;
using Salus.Models.Entities;

namespace Salus;

public class SalusDbContext : SalusDbContext<int>
{
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("EF Core isn't fully compatible with trimming, and running the application may generate unexpected runtime failures. Some specific coding pattern are usually required to make trimming work properly, see https://aka.ms/efcore-docs-trimming for more details.")]
    protected SalusDbContext(ISalus<int> salus) : base(salus)
    {
    }

    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("EF Core isn't fully compatible with trimming, and running the application may generate unexpected runtime failures. Some specific coding pattern are usually required to make trimming work properly, see https://aka.ms/efcore-docs-trimming for more details.")]
    protected SalusDbContext(ISalus<int> salus, DbContextOptions options) : base(salus, options)
    {
    }
}

/// <summary>
/// A DbContext class that can be used as the base for creating Salus DbContexts.
/// </summary>
public class SalusDbContext<TKey> : DbContext, ISalusDbContext<TKey>
{
    private readonly ISalusCore<TKey> _salus;
    private SalusDatabaseFacade<TKey>? _database;


    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("EF Core isn't fully compatible with trimming, and running the application may generate unexpected runtime failures. Some specific coding pattern are usually required to make trimming work properly, see https://aka.ms/efcore-docs-trimming for more details.")]
    protected SalusDbContext(ISalus<TKey> salus)
        : base()
    {
        _salus = (ISalusCore<TKey>)salus;
        _salus.Init(this);
    }

    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("EF Core isn't fully compatible with trimming, and running the application may generate unexpected runtime failures. Some specific coding pattern are usually required to make trimming work properly, see https://aka.ms/efcore-docs-trimming for more details.")]
    protected SalusDbContext(ISalus<TKey> salus, DbContextOptions options)
        : base(options)
    {
        _salus = (ISalusCore<TKey>)salus;
        _salus.Init(this);
    }

    public DbSet<SalusSaveEntity<TKey>> SalusSaves => Set<SalusSaveEntity<TKey>>();

    protected override sealed void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnSalusModelCreating(modelBuilder);
        _salus.OnModelCreating(modelBuilder);
    }

    protected virtual void OnSalusModelCreating(ModelBuilder modelBuilder)
    {
    }

    public override int SaveChanges() => SaveChanges(acceptAllChangesOnSuccess: true);

    
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        return _salus.SaveChanges(acceptAllChangesOnSuccess, base.SaveChanges);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await SaveChangesAsync(true, cancellationToken).ConfigureAwait(false);

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        return await _salus.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken, base.SaveChangesAsync).ConfigureAwait(false);
    }

    internal void Apply(Save<TKey> save)
    {
        _salus.Apply(save);
    }

    SalusDatabaseFacade<TKey> ISalusDbContext<TKey>.SalusDatabase
    {
        get
        {
            if (_database == null)
            {
                _database = new SalusDatabaseFacade<TKey>(base.Database, this, _salus);
            }
            return _database;
        }
    }

    public override DatabaseFacade Database => ((this as ISalusDbContext<TKey>).SalusDatabase);
}
