using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
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

        IDbContextTransaction? tran = null;
        try
        {
            if (Database.CurrentTransaction == null)
            {
                // If we're not already in a transaction, we create one here.
                // If we *are* already in a transaction, that transaction will be sufficient

                tran = Database.BeginTransaction();
            }

            base.SaveChanges(acceptAllChangesOnSuccess);

            if (result != null)
            {
                _salus.CompleteSave(result);
            }

            tran?.Commit();
        }
        catch
        {
            tran?.Rollback();
            throw;
        }
        finally
        {
            tran?.Dispose();
        }


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
