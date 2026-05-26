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
    Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Use many INSERT command. Need SaveChange()
    /// </summary>
    Task InsertManyAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Use many UPDATE command. Need SaveChange()
    /// </summary>
    Task UpdateManyAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    Task<TEntity> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Use many DELETE command. Need SaveChange()
    /// </summary>
    Task DeleteManyAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    Task<TEntity?> GetByIdAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task<IEnumerable<TEntity>> LoadAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
}
