using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<SalusDbContextTransaction<TKey>>? _logger;

    public SalusDbContextTransaction(IDbContextTransaction wrappedTransaction, ISalusCore<TKey> salus, ILogger<SalusDbContextTransaction<TKey>>? logger)
    {
        _wrappedTransaction = wrappedTransaction;
        _salus = salus;
        _transactionSaves = new List<Save<TKey>>();
        _logger = logger;
    }

    public Guid TransactionId => _wrappedTransaction.TransactionId;

    public void Commit()
    {
        _logger?.LogDebug("Commit transaction");
        _wrappedTransaction.Commit();
        OnCommitting();
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("Commit transaction async");
        await _wrappedTransaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        await OnCommittingAsync().ConfigureAwait(false);
    }

    public void Dispose()
    {
        _logger?.LogDebug("Dispose transaction");
        OnRollingBack();
        _wrappedTransaction.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        _logger?.LogDebug("Dispose transaction async");
        OnRollingBack();
        await _wrappedTransaction.DisposeAsync().ConfigureAwait(false);
    }

    public void Rollback()
    {
        _logger?.LogDebug("Rolling back transaction");
        OnRollingBack();
        _wrappedTransaction.Rollback();
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("Rolling back transaction async");
        OnRollingBack();
        await _wrappedTransaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
    }


    internal void AddTransactionSave(Save<TKey> save)
    {
        _transactionSaves.Add(save);
    }

    internal void OnCommitting()
    {
        foreach (var save in _transactionSaves)
        {
            _salus.SendMessage(save);
        }
    }

    internal async Task OnCommittingAsync()
    {
        foreach (var save in _transactionSaves)
        {
            await _salus.SendMessageAsync(save).ConfigureAwait(false);
        }
    }
    internal void OnRollingBack()
    {
        _transactionSaves.Clear();
    }
}
