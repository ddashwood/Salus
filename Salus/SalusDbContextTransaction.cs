using Microsoft.EntityFrameworkCore.Storage;
using Salus.Models.Changes;

namespace Salus;

/// <summary>
/// A DbContextTransaction that can be used internally by Salus - it
/// overrides those features of DatabaseFacade which require special
/// handling.
/// </summary>
internal class SalusDbContextTransaction<TKey> : IDbContextTransaction
{
    private readonly IDbContextTransaction _wrappedTransaction;
    private readonly ISalusCore<TKey> _salus;
    private readonly List<Save<TKey>> _transactionSaves;

    public SalusDbContextTransaction(IDbContextTransaction wrappedTransaction, ISalusCore<TKey> salus)
    {
        _wrappedTransaction = wrappedTransaction;
        _salus = salus;
        _transactionSaves = new List<Save<TKey>>();
    }

    public Guid TransactionId => _wrappedTransaction.TransactionId;

    public void Commit()
    {
        _wrappedTransaction.Commit();
        OnCommitting();
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await _wrappedTransaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        await OnCommittingAsync().ConfigureAwait(false);
    }

    public void Dispose()
    {
        OnRollingBack();
        _wrappedTransaction.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        OnRollingBack();
        await _wrappedTransaction.DisposeAsync().ConfigureAwait(false);
    }

    public void Rollback()
    {
        OnRollingBack();
        _wrappedTransaction.Rollback();
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        OnRollingBack();
        await _wrappedTransaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
    }


    public void AddTransactionSave(Save<TKey> save)
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

    public async Task OnCommittingAsync()
    {
        foreach (var save in _transactionSaves)
        {
            await _salus.SendMessageAsync(save).ConfigureAwait(false);
        }
    }
    public void OnRollingBack()
    {
        _transactionSaves.Clear();
    }
}
