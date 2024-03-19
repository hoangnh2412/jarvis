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

    Task InsertManyAsync(IEnumerable<TEntity> entities);

    Task<TEntity> UpdateAsync(TEntity entity);

    Task UpdateManyAsync(IEnumerable<TEntity> entities);

    Task<TEntity> DeleteAsync(TEntity entity);

    Task DeleteManyAsync(IEnumerable<TEntity> entities);
}