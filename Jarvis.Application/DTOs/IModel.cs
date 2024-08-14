using Jarvis.Application.DTOs;
using Jarvis.Domain.Common.Interfaces;

namespace Jarvis.Application.DTOs;

/// <summary>
/// The interface abstract model use when return search entity
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TModel"></typeparam>
public interface IModel<TEntity, TModel>
    where TEntity : IEntity
{
    TModel MapToModel(TEntity entity);
}