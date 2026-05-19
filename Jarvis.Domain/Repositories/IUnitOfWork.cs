using System.Data;
using Jarvis.Domain.DataStorages;

namespace Jarvis.Domain.Repositories;

/// <summary>
/// Unit of work over a storage context: repositories, save changes, transactions, and optional connection switching.
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    Task<IStorageContext> GetDbContextAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Switches tenant for this unit of work (pinned on UoW + ambient <see cref="ICurrentTenantAccessor"/>)
    /// and drops the cached context when the tenant changes.
    /// Does not create a <see cref="IStorageContext"/> — call <see cref="GetRepositoryAsync{TRepository}"/> or <see cref="GetDbContextAsync"/> afterward.
    /// </summary>
    Task SwitchDbContextAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<TRepository> GetRepositoryAsync<TRepository>(CancellationToken cancellationToken = default)
        where TRepository : IRepository;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<TTransaction> BeginTransactionAsync<TTransaction>(IsolationLevel isolation = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default);

    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    Task<TTransaction> UseTransactionAsync<TTransaction>(IDbTransaction transaction, CancellationToken cancellationToken = default);
}

/// <summary>
/// Unit of work bound to a specific storage context type.
/// </summary>
/// <typeparam name="TStorageContext"></typeparam>
public interface IUnitOfWork<TStorageContext> : IUnitOfWork where TStorageContext : IStorageContext
{
}
