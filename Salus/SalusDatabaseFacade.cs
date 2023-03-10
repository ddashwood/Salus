using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Salus.Models.Changes;

namespace Salus;

/// <summary>
/// A DatabaseFacade that can be used internally by Salus - it
/// overrides those features of DatabaseFacade which require special
/// handling.
/// </summary>
internal class SalusDatabaseFacade<TKey> : DatabaseFacade
{
    private readonly DatabaseFacade _wrappedFacade;
    private readonly ISalusCore<TKey> _salus;
    private readonly ILogger<SalusDatabaseFacade<TKey>>? _logger;
    private readonly ILoggerFactory? _loggerFactory;
    private SalusDbContextTransaction<TKey>? _currentTransaction;

    public SalusDatabaseFacade(DatabaseFacade wrappedFacade, DbContext context, ISalusCore<TKey> salus, ILoggerFactory? loggerFactory) 
        : base(context)
    {
        _wrappedFacade = wrappedFacade;
        _salus = salus;
        _loggerFactory = loggerFactory;
        _logger = _loggerFactory?.CreateLogger<SalusDatabaseFacade<TKey>>();
    }

    public override bool AutoSavepointsEnabled { get => _wrappedFacade.AutoSavepointsEnabled; set => _wrappedFacade.AutoSavepointsEnabled = value; }
    public override AutoTransactionBehavior AutoTransactionBehavior { get => _wrappedFacade.AutoTransactionBehavior; set => _wrappedFacade.AutoTransactionBehavior = value; }
    [Obsolete]
    public override bool AutoTransactionsEnabled { get => _wrappedFacade.AutoTransactionsEnabled; set => _wrappedFacade.AutoTransactionsEnabled = value; }
    public override IDbContextTransaction BeginTransaction()
    {
        _logger?.LogDebug("Begin transaction");
        var tran = _wrappedFacade.BeginTransaction();
        var tranLogger = _loggerFactory?.CreateLogger<SalusDbContextTransaction<TKey>>();
        _currentTransaction = new SalusDbContextTransaction<TKey>(tran, _salus, tranLogger);
        return _currentTransaction;
    }
    public override async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("Begin transaction async");
        var tran = await _wrappedFacade.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        var tranLogger = _loggerFactory?.CreateLogger<SalusDbContextTransaction<TKey>>();
        _currentTransaction = new SalusDbContextTransaction<TKey>(tran, _salus, tranLogger);
        return _currentTransaction;
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
        _logger?.LogDebug("Commit transaction");
        _wrappedFacade.CommitTransaction();
        OnCommitting();
    }
    public override async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("Commit transaction async");
        await _wrappedFacade.CommitTransactionAsync(cancellationToken).ConfigureAwait(false);
        await OnCommittingAsync().ConfigureAwait(false);
    }
    public override IExecutionStrategy CreateExecutionStrategy()
    {
        return _wrappedFacade.CreateExecutionStrategy();
    }
    public override IDbContextTransaction? CurrentTransaction
    {
        get
        {
            if (base.CurrentTransaction == null)
            {
                _currentTransaction = null;
                return null;
            }
            if (_currentTransaction == null)
            {
                return base.CurrentTransaction;
            }
            return _currentTransaction;
        }
    }
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
        _logger?.LogDebug("Rolling back transaction");
        OnRollingBack();
        _wrappedFacade.RollbackTransaction();
    }
    public override async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("Rolling back transaction async");
        OnRollingBack();
        await _wrappedFacade.RollbackTransactionAsync(cancellationToken).ConfigureAwait(false);
    }
    public override string ToString()
    {
        return _wrappedFacade.ToString()!;
    }

    public void AddTransactionSave(Save<TKey> save)
    {
        if (CurrentTransaction is SalusDbContextTransaction<TKey> salusTransaction)
        {
            salusTransaction.AddTransactionSave(save);
        }
        else if (CurrentTransaction == null)
        {
            throw new InvalidOperationException("Attempt to add to transaction saves without a current transaction");
        }
        else
        {
            throw new InvalidOperationException("Attempt to add to transactions saves but the current transaction does not support this");
        }
    }

    private void OnCommitting()
    {
        if (CurrentTransaction is SalusDbContextTransaction<TKey> salusTransaction)
        {
            salusTransaction.OnCommitting();
        }
    }

    private async Task OnCommittingAsync()
    {
        if (CurrentTransaction is SalusDbContextTransaction<TKey> salusTransaction)
        {
            await salusTransaction.OnCommittingAsync().ConfigureAwait(false);
        }
    }
    private void OnRollingBack()
    {
        if (CurrentTransaction is SalusDbContextTransaction<TKey> salusTransaction)
        {
            salusTransaction.OnRollingBack();
        }
    }
}
