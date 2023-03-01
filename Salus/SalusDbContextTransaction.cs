using Microsoft.EntityFrameworkCore.Storage;
using Salus.Models.Changes;

namespace Salus;

internal class SalusDbContextTransaction : IDbContextTransaction
{
    private readonly IDbContextTransaction _wrappedTransaction;
    private readonly ISalusCore _salus;
    private readonly List<Save> _transactionSaves;

    public SalusDbContextTransaction(IDbContextTransaction wrappedTransaction, ISalusCore salus)
    {
        _wrappedTransaction = wrappedTransaction;
        _salus = salus;
        _transactionSaves = new List<Save>();
    }

    public Guid TransactionId => _wrappedTransaction.TransactionId;

    public void Commit()
    {
        _wrappedTransaction.Commit();
        OnCommitting();
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await _wrappedTransaction.CommitAsync(cancellationToken);
        await OnCommittingAsync();
    }

    public void Dispose()
    {
        OnRollingBack();
        _wrappedTransaction.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        OnRollingBack();
        await _wrappedTransaction.DisposeAsync();
    }

    public void Rollback()
    {
        OnRollingBack();
        _wrappedTransaction.Rollback();
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        OnRollingBack();
        await _wrappedTransaction.RollbackAsync(cancellationToken);
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
