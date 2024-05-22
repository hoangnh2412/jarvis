using System.Linq.Expressions;
using Jarvis.Domain.Common.Interfaces;

namespace Jarvis.Application.Interfaces.Repositories;

/// <summary>
/// The interface abstracts method to handle write-only operation like Insert, Update, Delete
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface ICommandRepository<TEntity> : IRepository
    where TEntity : class, IEntity
{
    Task<TEntity> InsertAsync(TEntity entity);

    /// <summary>
    /// Use many INSERT command. Need SaveChange()
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    Task InsertManyAsync(IEnumerable<TEntity> entities);

    /// <summary>
    /// - SQLServer (or AzureSQL) under the hood uses SqlBulkCopy for Insert, Update/Delete = BulkInsert + raw Sql MERGE.
    /// - PostgreSQL (9.5+) is using COPY BINARY combined with ON CONFLICT for Update.
    /// - MySQL (8+) is using MySqlBulkCopy combined with ON DUPLICATE for Update.
    /// - SQLite has no Copy tool, instead library uses plain SQL combined with UPSERT.
    /// Without SaveChange()
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    Task InsertBatchAsync(IEnumerable<TEntity> entities);

    Task<TEntity> UpdateAsync(TEntity entity);

    /// <summary>
    /// Use many UPDATE command. Need SaveChange()
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task UpdateManyAsync(IEnumerable<TEntity> entities);

    /// <summary>
    /// - SQLServer (or AzureSQL) under the hood uses SqlBulkCopy for Insert, Update/Delete = BulkInsert + raw Sql MERGE.
    /// - PostgreSQL (9.5+) is using COPY BINARY combined with ON CONFLICT for Update.
    /// - MySQL (8+) is using MySqlBulkCopy combined with ON DUPLICATE for Update.
    /// - SQLite has no Copy tool, instead library uses plain SQL combined with UPSERT.
    /// Without SaveChange()
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    Task<int> UpdateBatchAsync(IQueryable<TEntity> queryable, Expression<Func<TEntity, TEntity>> updateFactory);
    Task UpdateBatchAsync(IEnumerable<TEntity> entities, Expression<Func<TEntity, TEntity>> updateFactory);

    Task<TEntity> DeleteAsync(TEntity entity);

    /// <summary>
    /// Use many DELETE command. Need SaveChange()
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task DeleteManyAsync(IEnumerable<TEntity> entities);

    /// <summary>
    /// - SQLServer (or AzureSQL) under the hood uses SqlBulkCopy for Insert, Update/Delete = BulkInsert + raw Sql MERGE.
    /// - PostgreSQL (9.5+) is using COPY BINARY combined with ON CONFLICT for Update.
    /// - MySQL (8+) is using MySqlBulkCopy combined with ON DUPLICATE for Update.
    /// - SQLite has no Copy tool, instead library uses plain SQL combined with UPSERT.
    /// Without SaveChange()
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    Task<int> DeleteBatchAsync(IQueryable<TEntity> queryable);
    Task DeleteBatchAsync(IEnumerable<TEntity> entities);
}