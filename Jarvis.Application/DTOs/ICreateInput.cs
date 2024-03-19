using Jarvis.Domain.Common.Interfaces;

namespace Jarvis.Application.DTOs;

/// <summary>
/// The interface abstract create model
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TModel"></typeparam>
public interface ICreateInput<TEntity, TModel>
    where TEntity : IEntity
{
    TEntity MapToEntity(TModel input);
}