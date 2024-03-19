using System.Linq.Expressions;
using Jarvis.Domain.Common.Interfaces;

namespace Jarvis.Application.Interfaces.Repositories;

/// <summary>
/// The interface abstracts method to handle read-only operation
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IQueryRepository<TEntity> : IRepository
    where TEntity : class, IEntity
{
    IQueryable<TEntity> GetQuery(bool asNoTracking = false);

    Task<ICollection<TEntity>> ListAsync(Expression<Func<TEntity, bool>> predicate = null, bool asNoTracking = false);

    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate = null, bool asNoTracking = false);

    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate = null, bool asNoTracking = false);
}