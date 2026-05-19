using System.Data;
using Jarvis.Domain.DataStorages;

namespace Jarvis.Domain.Repositories;

/// <summary>
/// Coordinates a single storage session: resolves repositories, persists changes, manages transactions,
/// and optionally switches tenant for multi-tenant <see cref="IStorageContext"/> implementations.
/// </summary>
/// <remarks>
/// Typical lifetime is scoped (per HTTP request, per message, or per explicit DI scope).
/// Disposing the unit of work disposes the underlying storage context and any tenant scope opened by
/// <see cref="SwitchDbContextAsync"/>.
/// </remarks>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Returns the storage context for this unit of work, creating or reusing a cached instance.
    /// </summary>
    /// <remarks>
    /// Tenant id is resolved on this UoW only: explicit <see cref="SwitchDbContextAsync"/>, then
    /// <see cref="ITenantIdResolverFactory"/> (header, claim, query, host). Ambient
    /// <see cref="ICurrentTenantAccessor"/> is not used here (avoids cross-UoW leakage when Master and tenant
    /// UoWs share a request); the accessor is set by <see cref="SwitchDbContextAsync"/> for connection resolution.
    /// Prefer <see cref="GetRepositoryAsync"/> when working through repositories.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The active <see cref="IStorageContext"/> for this unit of work.</returns>
    Task<IStorageContext> GetDbContextAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Switches tenant for this unit of work (pinned on this UoW and ambient <see cref="ICurrentTenantAccessor"/>
    /// for dedicated-database connection opening) and disposes the cached storage context when the tenant changes.
    /// </summary>
    /// <remarks>
    /// Does not create a <see cref="IStorageContext"/>; call <see cref="GetRepositoryAsync"/> or
    /// <see cref="GetDbContextAsync"/> afterward so a new context is created for the target tenant.
    /// <para>
    /// Repositories obtained before this call still reference the previous context via
    /// <see cref="IRepository.SetStorageContext"/>. After switching, always resolve repositories again;
    /// using an old instance can read or write the wrong tenant database.
    /// </para>
    /// </remarks>
    /// <param name="tenantId">Target tenant id. Must not be <see cref="Guid.Empty"/>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SwitchDbContextAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves a repository from DI and binds it to the current storage context for this unit of work.
    /// </summary>
    /// <typeparam name="TRepository">Repository service type (must implement <see cref="IRepository"/>).</typeparam>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A repository instance with <see cref="IRepository.SetStorageContext"/> already called.</returns>
    Task<TRepository> GetRepositoryAsync<TRepository>(CancellationToken cancellationToken = default)
        where TRepository : IRepository;

    /// <summary>
    /// Persists all pending changes tracked by the underlying storage context.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts a database transaction on the underlying storage context.
    /// </summary>
    /// <typeparam name="TTransaction">Provider-specific transaction type (for example, EF Core <c>IDbContextTransaction</c>).</typeparam>
    /// <param name="isolation">Transaction isolation level.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The started transaction.</returns>
    Task<TTransaction> BeginTransactionAsync<TTransaction>(
        IsolationLevel isolation = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction on the underlying storage context.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction on the underlying storage context.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Enlists an existing <see cref="IDbTransaction"/> on the underlying storage context.
    /// </summary>
    /// <typeparam name="TTransaction">Provider-specific transaction type returned after enlistment.</typeparam>
    /// <param name="transaction">The transaction to enlist (provider <see cref="System.Data.Common.DbTransaction"/>).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The context transaction wrapper.</returns>
    Task<TTransaction> UseTransactionAsync<TTransaction>(
        IDbTransaction transaction,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Unit of work bound to a specific <see cref="IStorageContext"/> implementation (for example, a named DbContext).
/// </summary>
/// <typeparam name="TStorageContext">Concrete storage context type for this unit of work.</typeparam>
public interface IUnitOfWork<TStorageContext> : IUnitOfWork
    where TStorageContext : IStorageContext
{
}
