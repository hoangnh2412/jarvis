using System.Linq.Expressions;
using Jarvis.Domain.Entities;

namespace Jarvis.Domain.Repositories;

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

    Task<TEntity> UpdateAsync(TEntity entity);

    /// <summary>
    /// Use many UPDATE command. Need SaveChange()
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    Task UpdateManyAsync(IEnumerable<TEntity> entities);

    Task<TEntity> DeleteAsync(TEntity entity);

    /// <summary>
    /// Use many DELETE command. Need SaveChange()
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    Task DeleteManyAsync(IEnumerable<TEntity> entities);
}