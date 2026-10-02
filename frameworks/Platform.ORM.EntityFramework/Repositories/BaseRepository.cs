using Platform.DDD.Domain.Entities;
using Platform.DDD.Domain.Repositories;

namespace Platform.ORM.EntityFramework.Repositories;

/// <summary>
/// Full read/write repository when CQRS split is not used.
/// </summary>
public class BaseRepository<TEntity> : EfRepositoryCore<TEntity>, IRepository<TEntity>
    where TEntity : class, IEntity
{
}
