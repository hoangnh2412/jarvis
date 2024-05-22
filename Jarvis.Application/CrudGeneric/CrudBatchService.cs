using System.Linq.Expressions;
using Jarvis.Application.DTOs;
using Jarvis.Application.Interfaces;
using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Domain.Common.Interfaces;

namespace Jarvis.Application.CrudGeneric;

/// <summary>
/// Generic Service with Create, Update, Delete, Pagination of a Entity
/// </summary>
/// <typeparam name="TUnitOfWork">Type of UnitOfWork contains Entity</typeparam>
/// <typeparam name="TKey">Data type field Id of Entity</typeparam>
/// <typeparam name="TEntity">Type of Entity</typeparam>
/// <typeparam name="TModel">Type of model return for method Get</typeparam>
/// <typeparam name="TPagingInput">Type of input model for method Pagination</typeparam>
/// <typeparam name="TPagingOutput">Type of output model for method Pagination</typeparam>
/// <typeparam name="TCreateInput">Type of input model for method Create</typeparam>
/// <typeparam name="TUpdateInput">Type of input model for method Update</typeparam>
public abstract partial class CrudService<TUnitOfWork, TKey, TEntity, TModel, TPagingInput, TPagingOutput, TCreateInput, TUpdateInput>
    : ICrudService<TUnitOfWork, TKey, TEntity, TModel, TPagingInput, TPagingOutput, TCreateInput, TUpdateInput>
    where TUnitOfWork : IUnitOfWork
    where TEntity : class, IEntity<TKey>, ILogCreatedEntity
    where TPagingInput : IPaging
    where TCreateInput : ICreateInput<TEntity, TCreateInput>
{
    public async Task<IEnumerable<TModel>> CreateManyAsync(IEnumerable<TCreateInput> input)
    {
        await OnCreateBeginAsync(input);

        var repo = _uow.GetRepository<IRepository<TEntity>>();

        var entities = new List<TEntity>();
        foreach (var item in input)
        {
            var entity = MapToEntity(item);

            await OnBeforeSaveChangesInsertAsync(item, entity);
            entities.Add(entity);
        }

        await repo.InsertManyAsync(entities);
        var result = await _uow.CommitAsync();

        if (result > 0)
            await OnCreateSuccessAsync(input, entities);
        else
            await OnCreateFailAsync(input, entities);

        await OnCreateFinishAsync(input, entities);

        return entities.Select(x => MapToModel(x));
    }

    /// <summary>
    /// Function handle when creating
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public virtual Task OnCreateBeginAsync(IEnumerable<TCreateInput> input)
    {
        return Task.CompletedTask;
    }

    public virtual Task OnCreateSuccessAsync(IEnumerable<TCreateInput> input, IEnumerable<TEntity> entity)
    {
        return Task.CompletedTask;
    }

    public virtual Task OnBeforeSaveChangesInsertAsync(IEnumerable<TCreateInput> input, IEnumerable<TEntity> entities)
    {
        return Task.CompletedTask;
    }

    public virtual Task OnCreateFinishAsync(IEnumerable<TCreateInput> input, IEnumerable<TEntity> entity)
    {
        return Task.CompletedTask;
    }

    public virtual Task OnCreateFailAsync(IEnumerable<TCreateInput> input, IEnumerable<TEntity> entity)
    {
        return Task.CompletedTask;
    }




    public Task<TModel> UpdateManyAsync(TKey id, TUpdateInput input, bool asNoQuery = false)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<TModel>> UpdateBatchAsync(Expression<Func<TEntity, bool>> predicate)
    {
        // var repo = _uow.GetRepository<IRepository<TEntity>>();
        // await repo.UpdateBatchAsync(predicate, null);
        await Task.Yield();
        return null;

    }

    public virtual Task OnUpdateBeginAsync(IEnumerable<TUpdateInput> input)
    {
        return Task.CompletedTask;
    }



    public Task<IEnumerable<TModel>> DeleteBatchAsync(Expression<Func<TEntity, bool>> predicate, bool asNoQuery = false)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<TModel>> DeleteBatchAsync(Func<TEntity, bool> predicate, bool asNoQuery = false)
    {
        throw new NotImplementedException();
    }
}