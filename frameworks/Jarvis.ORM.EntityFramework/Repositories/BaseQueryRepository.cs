using Jarvis.DDD.Domain.Entities;
using Jarvis.DDD.Domain.Repositories;

namespace Jarvis.ORM.EntityFramework.Repositories;

/// <summary>
/// Read-only repository (CQRS query side). Uses no-tracking queries from <see cref="EfRepositoryCore{TEntity}"/>.
/// </summary>
public class BaseQueryRepository<TEntity> : EfRepositoryCore<TEntity>, IQueryRepository<TEntity>
    where TEntity : class, IEntity
{
}
