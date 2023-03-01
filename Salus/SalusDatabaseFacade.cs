using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Salus.Models.Changes;

namespace Salus;

public class SalusDatabaseFacade : DatabaseFacade, ISalusTransactionSaver
{
    private readonly DatabaseFacade _wrappedFacade;
    private readonly List<Save> _transactionSaves;
    private readonly ISalusCore _salus;

    public SalusDatabaseFacade(DatabaseFacade wrappedFacade, DbContext context, ISalusCore salus) : base(context)
    {
        _wrappedFacade = wrappedFacade;
        _transactionSaves = new List<Save>();
        _salus = salus;
    }

    public override bool AutoSavepointsEnabled { get => _wrappedFacade.AutoSavepointsEnabled; set => _wrappedFacade.AutoSavepointsEnabled = value; }
    public override AutoTransactionBehavior AutoTransactionBehavior { get => _wrappedFacade.AutoTransactionBehavior; set => _wrappedFacade.AutoTransactionBehavior = value; }
    [Obsolete]
    public override bool AutoTransactionsEnabled { get => _wrappedFacade.AutoTransactionsEnabled; set => _wrappedFacade.AutoTransactionsEnabled = value; }
    public override IDbContextTransaction BeginTransaction()
    {
        var tran = _wrappedFacade.BeginTransaction();
        return new SalusDbContextTransaction(tran, this);
    }
    public override async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var tran = await _wrappedFacade.BeginTransactionAsync(cancellationToken);
        return new SalusDbContextTransaction(tran, this);
    }
    public override bool CanConnect()
    {
        return _wrappedFacade.CanConnect();
    }
    public override Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
    {
        return _wrappedFacade.CanConnectAsync(cancellationToken);
    }
    public override void CommitTransaction()
    {
        _wrappedFacade.CommitTransaction();
    }
    public override Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        return _wrappedFacade.CommitTransactionAsync(cancellationToken);
    }
    public override IExecutionStrategy CreateExecutionStrategy()
    {
        return _wrappedFacade.CreateExecutionStrategy();
    }
    public override IDbContextTransaction? CurrentTransaction => _wrappedFacade.CurrentTransaction;
    public override bool EnsureCreated()
    {
        return _wrappedFacade.EnsureCreated();
    }
    public override Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = default)
    {
        return _wrappedFacade.EnsureCreatedAsync(cancellationToken);
    }
    public override bool EnsureDeleted()
    {
        return _wrappedFacade.EnsureDeleted();
    }
    public override Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = default)
    {
        return _wrappedFacade.EnsureDeletedAsync(cancellationToken);
    }
    public override bool Equals(object? obj)
    {
        return _wrappedFacade.Equals(obj);
    }
    public override int GetHashCode()
    {
        return _wrappedFacade.GetHashCode();
    }
    public override string? ProviderName => _wrappedFacade.ProviderName;
    public override void RollbackTransaction()
    {
        OnRollingBack();
        _wrappedFacade.RollbackTransaction();
    }
    public override Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        OnRollingBack();
        return _wrappedFacade.RollbackTransactionAsync(cancellationToken);
    }
    public override string ToString()
    {
        return _wrappedFacade.ToString()!;
    }

    public void AddTransactionSave(Save save)
    {
        _transactionSaves.Add(save);
    }

    public void OnCommitting()
    {
        foreach (var save in _transactionSaves)
        {
            _salus.SendMessages(save);
        }
    }

    public Task OnCommittingAsync()
    {
        throw new NotImplementedException();
    }
    public void OnRollingBack()
    {
        _transactionSaves.Clear();
    }
}
