using Jarvis.Domain.Common.Interfaces;

namespace Jarvis.Application.DTOs;

/// <summary>
/// The interface abstract update input model
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TModel"></typeparam>
public interface IUpdateInput<TEntity, TModel>
    where TEntity : IEntity
{
    TEntity MapToEntity(TModel input, TEntity entity);
}