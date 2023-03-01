using Microsoft.EntityFrameworkCore.Storage;

namespace Salus;

internal class SalusDbContextTransaction : IDbContextTransaction
{
    private readonly IDbContextTransaction _wrappedTransaction;
    private readonly ISalusTransactionSaver _saver;

    public SalusDbContextTransaction(IDbContextTransaction wrappedTransaction, ISalusTransactionSaver saver)
    {
        _wrappedTransaction = wrappedTransaction;
        _saver = saver;
    }

    public Guid TransactionId => _wrappedTransaction.TransactionId;

    public void Commit()
    {
        _wrappedTransaction.Commit();
        _saver.OnCommitting();
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await _wrappedTransaction.CommitAsync(cancellationToken);
        await _saver.OnCommittingAsync();
    }

    public void Dispose()
    {
        _saver.OnRollingBack();
        _wrappedTransaction.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        _saver.OnRollingBack();
        await _wrappedTransaction.DisposeAsync();
    }

    public void Rollback()
    {
        _saver.OnRollingBack();
        _wrappedTransaction.Rollback();
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        _saver.OnRollingBack();
        await _wrappedTransaction.RollbackAsync(cancellationToken);
    }
}
