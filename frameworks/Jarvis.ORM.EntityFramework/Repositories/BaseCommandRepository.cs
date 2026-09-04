using Jarvis.DDD.Domain.Entities;
using Jarvis.DDD.Domain.Repositories;

namespace Jarvis.ORM.EntityFramework.Repositories;

/// <summary>
/// Write-only repository (CQRS command side).
/// </summary>
public class BaseCommandRepository<TEntity> : EfRepositoryCore<TEntity>, ICommandRepository<TEntity>
    where TEntity : class, IEntity
{
}
