using Jarvis.Domain.Entities;
using Jarvis.Domain.Repositories;

namespace Jarvis.EntityFramework.Repositories;

/// <summary>
/// Full read/write repository when CQRS split is not used.
/// </summary>
public class BaseRepository<TEntity> : EfRepositoryCore<TEntity>, IRepository<TEntity>
    where TEntity : class, IEntity
{
}
