using System.Data;
using System.Data.Common;
using Jarvis.Domain.DataStorages;
using Jarvis.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.EntityFramework.Repositories;

public abstract class BaseUnitOfWork<T>(
    IServiceProvider services,
    IDbContextFactory<T> factory,
    ITenantIdResolverFactory tenantIdResolverFactory,
    ICurrentTenantAccessor currentTenantAccessor)
    : IUnitOfWork<T> where T : DbContext, IStorageContext
{
    private readonly IServiceProvider _services = services;
    private readonly IDbContextFactory<T> _factory = factory;
    private readonly ITenantIdResolverFactory _tenantIdResolverFactory = tenantIdResolverFactory;
    private readonly ICurrentTenantAccessor _currentTenantAccessor = currentTenantAccessor;
    protected DbContext? StorageContext { get; private set; }
    private Guid? _switchedTenantId;
    private Guid? _contextTenantId;
    private IDisposable? _tenantScope;
    private bool _disposed;

    public async Task<IStorageContext> GetDbContextAsync(CancellationToken cancellationToken = default)
    {
        await EnsureDbContextAsync(cancellationToken).ConfigureAwait(false);
        return (IStorageContext)StorageContext!;
    }

    public async Task SwitchDbContextAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId is required.", nameof(tenantId));

        cancellationToken.ThrowIfCancellationRequested();
        _switchedTenantId = tenantId;
        _tenantScope?.Dispose();
        _tenantScope = _currentTenantAccessor.BeginScope(tenantId);

        if (StorageContext != null && _contextTenantId != tenantId)
        {
            await StorageContext.DisposeAsync().ConfigureAwait(false);
            StorageContext = null;
            _contextTenantId = null;
        }
    }

    public async Task<TRepository> GetRepositoryAsync<TRepository>(CancellationToken cancellationToken = default)
        where TRepository : IRepository
    {
        var repo = (TRepository)_services.GetRequiredService(typeof(TRepository))
            ?? throw new NullReferenceException($"Service name {typeof(TRepository).Name} can't resolve");
        var context = await GetDbContextAsync(cancellationToken).ConfigureAwait(false);
        repo.SetStorageContext(context);
        return repo;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var dbContext = await EnsureDbContextAsync(cancellationToken).ConfigureAwait(false);
        return await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<TTransaction> BeginTransactionAsync<TTransaction>(
        IsolationLevel isolation = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await EnsureDbContextAsync(cancellationToken).ConfigureAwait(false);
        var transaction = await dbContext.Database.BeginTransactionAsync(isolation, cancellationToken).ConfigureAwait(false);
        return (TTransaction)transaction;
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        var dbContext = await EnsureDbContextAsync(cancellationToken).ConfigureAwait(false);
        await dbContext.Database.CommitTransactionAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        var dbContext = await EnsureDbContextAsync(cancellationToken).ConfigureAwait(false);
        await dbContext.Database.RollbackTransactionAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<TTransaction> UseTransactionAsync<TTransaction>(
        IDbTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        if (transaction is not DbTransaction dbTransaction)
            throw new ArgumentException("Transaction must be a DbTransaction.", nameof(transaction));

        var dbContext = await EnsureDbContextAsync(cancellationToken).ConfigureAwait(false);
        var contextTransaction = await dbContext.Database.UseTransactionAsync(dbTransaction, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Failed to enlist the transaction on the DbContext.");
        return (TTransaction)contextTransaction;
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        _tenantScope?.Dispose();
        _tenantScope = null;
        _switchedTenantId = null;
        if (StorageContext != null)
        {
            StorageContext.Dispose();
            StorageContext = null;
            _contextTenantId = null;
        }

        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;
        _disposed = true;
        _tenantScope?.Dispose();
        _tenantScope = null;
        _switchedTenantId = null;
        if (StorageContext != null)
        {
            await StorageContext.DisposeAsync().ConfigureAwait(false);
            StorageContext = null;
            _contextTenantId = null;
        }

        GC.SuppressFinalize(this);
    }

    private async Task<DbContext> EnsureDbContextAsync(CancellationToken cancellationToken)
    {
        var tenantId = await ResolveTenantIdAsync(cancellationToken).ConfigureAwait(false);

        if (StorageContext != null && _contextTenantId == tenantId)
        {
            ApplyTenantId(tenantId);
            return StorageContext;
        }

        if (StorageContext != null)
        {
            await StorageContext.DisposeAsync().ConfigureAwait(false);
            StorageContext = null;
            _contextTenantId = null;
        }

        StorageContext = await _factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        _contextTenantId = tenantId;
        ApplyTenantId(tenantId);
        return StorageContext;
    }

    private async Task<Guid?> ResolveTenantIdAsync(CancellationToken cancellationToken)
    {
        if (_switchedTenantId.HasValue)
            return _switchedTenantId;

        if (_currentTenantAccessor.TenantId.HasValue)
            return _currentTenantAccessor.TenantId;

        return await _tenantIdResolverFactory.GetTenantIdAsync(cancellationToken).ConfigureAwait(false);
    }

    private void ApplyTenantId(Guid? tenantId)
    {
        if (StorageContext == null || !tenantId.HasValue)
            return;

        ((IStorageContext)StorageContext).SetTenantId(tenantId);
    }
}
