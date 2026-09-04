using Jarvis.DDD.Domain.Entities;
using Jarvis.DDD.Domain.Repositories;

namespace Jarvis.ORM.EntityFramework.Repositories;

/// <summary>
/// Full read/write repository when CQRS split is not used.
/// </summary>
public class BaseRepository<TEntity> : EfRepositoryCore<TEntity>, IRepository<TEntity>
    where TEntity : class, IEntity
{
}
