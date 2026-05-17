using Jarvis.Domain.Entities;
using Jarvis.Domain.Repositories;

namespace Jarvis.EntityFramework.Repositories;

/// <summary>
/// Write-only repository (CQRS command side).
/// </summary>
public class BaseCommandRepository<TEntity> : EfRepositoryCore<TEntity>, ICommandRepository<TEntity>
    where TEntity : class, IEntity
{
}
