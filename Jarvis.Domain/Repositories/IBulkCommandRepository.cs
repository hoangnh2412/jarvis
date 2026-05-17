using System.Linq.Expressions;
using Jarvis.Domain.Entities;

namespace Jarvis.Domain.Repositories;

/// <summary>
/// The interface abstracts method to handle write-only operation like Insert, Update, Delete
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IBulkCommandRepository<TEntity> : IRepository
    where TEntity : class, IEntity
{
    /// <summary>
    /// - SQLServer (or AzureSQL) under the hood uses SqlBulkCopy for Insert, Update/Delete = BulkInsert + raw Sql MERGE.
    /// - PostgreSQL (9.5+) is using COPY BINARY combined with ON CONFLICT for Update.
    /// - MySQL (8+) is using MySqlBulkCopy combined with ON DUPLICATE for Update.
    /// - SQLite has no Copy tool, instead library uses plain SQL combined with UPSERT.
    /// Without SaveChange()
    /// </summary>
    Task InsertBatchAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// - SQLServer (or AzureSQL) under the hood uses SqlBulkCopy for Insert, Update/Delete = BulkInsert + raw Sql MERGE.
    /// - PostgreSQL (9.5+) is using COPY BINARY combined with ON CONFLICT for Update.
    /// - MySQL (8+) is using MySqlBulkCopy combined with ON DUPLICATE for Update.
    /// - SQLite has no Copy tool, instead library uses plain SQL combined with UPSERT.
    /// Without SaveChange()
    /// </summary>
    Task<int> UpdateBatchAsync(
        IQueryable<TEntity> queryable,
        Expression<Func<TEntity, TEntity>> updateFactory,
        CancellationToken cancellationToken = default);

    Task UpdateBatchAsync(
        IEnumerable<TEntity> entities,
        Expression<Func<TEntity, TEntity>> updateFactory,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// - SQLServer (or AzureSQL) under the hood uses SqlBulkCopy for Insert, Update/Delete = BulkInsert + raw Sql MERGE.
    /// - PostgreSQL (9.5+) is using COPY BINARY combined with ON CONFLICT for Update.
    /// - MySQL (8+) is using MySqlBulkCopy combined with ON DUPLICATE for Update.
    /// - SQLite has no Copy tool, instead library uses plain SQL combined with UPSERT.
    /// Without SaveChange()
    /// </summary>
    Task<int> DeleteBatchAsync(IQueryable<TEntity> queryable, CancellationToken cancellationToken = default);

    Task DeleteBatchAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
}
