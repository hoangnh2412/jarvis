using Jarvis.Application.DTOs;
using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Domain.Common.Interfaces;

namespace Jarvis.Application.CrudGeneric;

/// <summary>
/// The interface generic Service with Create, Update, Delete, Pagination of a Entity
/// </summary>
/// <typeparam name="TUnitOfWork">Type of UnitOfWork contains Entity</typeparam>
/// <typeparam name="TKey">Data type field Id of Entity</typeparam>
/// <typeparam name="TEntity">Type of Entity</typeparam>
/// <typeparam name="TModel">Type of model return for method Get</typeparam>
/// <typeparam name="TPagingInput">Type of input model for method Pagination</typeparam>
/// <typeparam name="TPagingOutput">Type of output model for method Pagination</typeparam>
/// <typeparam name="TCreateInput">Type of input model for method Create</typeparam>
/// <typeparam name="TUpdateInput">Type of input model for method Update</typeparam>
public interface ICrudService<TUnitOfWork, TKey, TEntity, TModel, TPagingInput, TPagingOutput, TCreateInput, TUpdateInput>
    where TUnitOfWork : IUnitOfWork
    where TEntity : class, IEntity<TKey>, ILogCreatedEntity
    where TPagingInput : IPaging
    where TCreateInput : ICreateInput<TEntity, TCreateInput>
{
    /// <summary>
    /// Return list of items after pagination
    /// </summary>
    /// <param name="paging">Search parameters</param>
    /// <param name="asNoTracking">If asNoTracking is true then DbContext not tracking change of entity</param>
    /// <returns></returns>
    Task<IPaged<TPagingOutput>> PaginationAsync(TPagingInput paging, bool asNoTracking = false);

    /// <summary>
    /// Return a item search by Key
    /// </summary>
    /// <param name="id"></param>
    /// <param name="asNoTracking">If asNoTracking is true then DbContext not tracking change of entity</param>
    /// <returns></returns>
    // Task<TModel> GetByKeyAsync(Guid key, bool asNoTracking = false);

    /// <summary>
    /// Return a item search by Code
    /// </summary>
    /// <param name="code"></param>
    /// <param name="asNoTracking">If asNoTracking is true then DbContext not tracking change of entity</param>
    /// <returns></returns>
    Task<TModel> GetByCodeAsync(string code, bool asNoTracking = false);

    /// <summary>
    /// Return a item search by Id
    /// </summary>
    /// <param name="key"></param>
    /// <param name="asNoTracking">If asNoTracking is true then DbContext not tracking change of entity</param>
    /// <returns></returns>
    Task<TModel> GetByIdAsync(TKey id, bool asNoTracking = false);

    /// <summary>
    /// Create new item
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<TModel> CreateAsync(TCreateInput input);

    /// <summary>
    /// Create new items
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<IEnumerable<TModel>> CreateBatchAsync(IEnumerable<TCreateInput> input);

    /// <summary>
    /// Update a item by Key
    /// </summary>
    /// <param name="key"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    // Task<TModel> UpdateByKeyAsync(Guid key, TUpdateInput input);

    /// <summary>
    /// Update a item by Id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<TModel> UpdateAsync(TKey id, TUpdateInput input, bool asNoQuery = false);

    /// <summary>
    /// Update a item by Id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<IEnumerable<TModel>> UpdateBatchAsync(IEnumerable<TUpdateInput> input, bool asNoQuery = false);

    /// <summary>
    /// Delete a item by Id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="asNoQuery">Set true if without query first</param>
    /// <returns></returns>
    Task<TModel> DeleteByIdAsync(TKey id, bool asNoQuery = false);

    /// <summary>
    /// Delete a item by Key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    // Task<TModel> DeleteByKeyAsync(Guid key);
}