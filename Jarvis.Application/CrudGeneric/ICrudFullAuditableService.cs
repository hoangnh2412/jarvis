using Jarvis.Application.DTOs;
using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Domain.Common.Interfaces;

namespace Jarvis.Application.CrudGeneric;

/// <summary>
/// The interface generic Service with Create, Update, Delete, Pagination, Trash (soft delete) of a Entity
/// </summary>
/// <typeparam name="TUnitOfWork">Type of UnitOfWork contains Entity</typeparam>
/// <typeparam name="TKey">Data type field Id of Entity</typeparam>
/// <typeparam name="TEntity">Type of Entity</typeparam>
/// <typeparam name="TModel">Type of model return for method Get</typeparam>
/// <typeparam name="TPagingInput">Type of input model for method Pagination</typeparam>
/// <typeparam name="TPagingOutput">Type of output model for method Pagination</typeparam>
/// <typeparam name="TCreateInput">Type of input model for method Create</typeparam>
/// <typeparam name="TUpdateInput">Type of input model for method Update</typeparam>
public interface ICrudFullAuditableService<TUnitOfWork, TKey, TEntity, TModel, TPagingInput, TPagingOutput, TCreateInput, TUpdateInput>
    : ICrudService<TUnitOfWork, TKey, TEntity, TModel, TPagingInput, TPagingOutput, TCreateInput, TUpdateInput>
    where TUnitOfWork : IUnitOfWork
    where TEntity : class, IEntity<TKey>, ILogCreatedEntity, ILogUpdatedEntity, ILogDeletedEntity<TKey>
    where TPagingInput : IPaging
    where TCreateInput : ICreateInput<TEntity, TCreateInput>
{
    /// <summary>
    /// Soft delete by Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<int> TrashByIdAsync(TKey id);

    /// <summary>
    /// Soft delete by Key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    // Task<int> TrashByKeyAsync(Guid key);
}