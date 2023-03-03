using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Salus.Models;
using Salus.Models.Changes;

namespace Salus;

public class SalusDbContext : DbContext, ISalusDbContext
{
    private readonly ISalusCore _salus;
    private SalusDatabaseFacade? _database;


    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("EF Core isn't fully compatible with trimming, and running the application may generate unexpected runtime failures. Some specific coding pattern are usually required to make trimming work properly, see https://aka.ms/efcore-docs-trimming for more details.")]
    protected SalusDbContext(ISalus salus)
        : base()
    {
        _salus = (ISalusCore)salus;
        _salus.Init(this);
    }

    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("EF Core isn't fully compatible with trimming, and running the application may generate unexpected runtime failures. Some specific coding pattern are usually required to make trimming work properly, see https://aka.ms/efcore-docs-trimming for more details.")]
    protected SalusDbContext(ISalus salus, DbContextOptions options)
        : base(options)
    {
        _salus = (ISalusCore)salus;
        _salus.Init(this);
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
        var result = _salus.BuildPreliminarySave();
        base.SaveChanges(acceptAllChangesOnSuccess);

        if (result != null)
        {
            _salus.CompleteSave(result);
        }

        // TO DO - The separate SaveChanges above needs to be wrapped in a transaction
        // with the previous SaveChanges, unless we are already in a transaction



        if (result == null)
        {
            return 0;
        }

        if (Database.CurrentTransaction == null)
        {
            _salus.SendMessages(result);
        }
        else
        {
            SalusDatabase.AddTransactionSave(result);
        }
        return result.Changes.Count;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await SaveChangesAsync(true, cancellationToken);

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();

        //var result = await _salus.SaveChangesAsync(cancellationToken);
        //await base.SaveChangesAsync(acceptAllChangesOnSuccess);

        //if (result == null)
        //{
        //    return 0;
        //}

        //if (Database.CurrentTransaction == null)
        //{
        //    _salus.SendMessages(result);
        //}
        //else
        //{
        //    SalusDatabase.AddTransactionSave(result);
        //}
        //return result.Changes.Count;
    }

    internal void Apply(Save save)
    {
        _salus.Apply(save);
    }

    private SalusDatabaseFacade SalusDatabase
    {
        get
        {
            if (_database == null)
            {
                _database = new SalusDatabaseFacade(base.Database, this, _salus);
            }
            return _database;
        }
    }

    public override DatabaseFacade Database => SalusDatabase;
}
