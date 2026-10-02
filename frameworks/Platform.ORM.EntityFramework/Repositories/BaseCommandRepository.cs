using Platform.DDD.Domain.Entities;
using Platform.DDD.Domain.Repositories;

namespace Platform.ORM.EntityFramework.Repositories;

/// <summary>
/// Write-only repository (CQRS command side).
/// </summary>
public class BaseCommandRepository<TEntity> : EfRepositoryCore<TEntity>, ICommandRepository<TEntity>
    where TEntity : class, IEntity
{
}
