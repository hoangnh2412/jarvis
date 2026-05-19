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
    ITenantIdResolverFactory tenantIdResolverFactory)
    : IUnitOfWork<T> where T : DbContext, IStorageContext
{
    private readonly IServiceProvider _services = services;
    private readonly IDbContextFactory<T> _factory = factory;
    private readonly ITenantIdResolverFactory _tenantIdResolverFactory = tenantIdResolverFactory;
    protected DbContext? StorageContext { get; private set; }
    private bool _disposed;

    public async Task<IStorageContext> GetDbContextAsync(CancellationToken cancellationToken = default)
    {
        await EnsureDbContextAsync(cancellationToken).ConfigureAwait(false);
        return (IStorageContext)StorageContext!;
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
        if (StorageContext != null)
        {
            StorageContext.Dispose();
            StorageContext = null;
        }

        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;
        _disposed = true;
        if (StorageContext != null)
        {
            await StorageContext.DisposeAsync().ConfigureAwait(false);
            StorageContext = null;
        }

        GC.SuppressFinalize(this);
    }

    private async Task<DbContext> EnsureDbContextAsync(CancellationToken cancellationToken)
    {
        if (StorageContext != null)
            return StorageContext;

        StorageContext = await _factory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var tenantId = await _tenantIdResolverFactory.GetTenantIdAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        if (tenantId.HasValue)
            ((IStorageContext)StorageContext).SetTenantId(tenantId);
        return StorageContext;
    }
}
