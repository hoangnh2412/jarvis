using Jarvis.Application.DTOs;
using Jarvis.Domain.Common.Interfaces;

namespace Jarvis.Application.DTOs;

/// <summary>
/// The interface abstract pagination output model
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TModel"></typeparam>
public interface IPagingOutput<TEntity, TModel>
    where TEntity : IEntity
{
    TModel MapToModel(TEntity entity);
}