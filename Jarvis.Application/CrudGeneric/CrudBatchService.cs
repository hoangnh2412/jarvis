using Microsoft.EntityFrameworkCore;
using Jarvis.Application.DTOs;
using Jarvis.Application.Extensions;
using Jarvis.Application.Interfaces;
using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Domain.Attributes;
using Jarvis.Domain.Common;
using Jarvis.Domain.Common.Interfaces;
using Jarvis.Shared.Extensions;

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
public abstract class CrudBatchService<TUnitOfWork, TKey, TEntity, TModel, TPagingInput, TPagingOutput, TCreateInput, TUpdateInput>
    : CrudService<TUnitOfWork, TKey, TEntity, TModel, TPagingInput, TPagingOutput, TCreateInput, TUpdateInput>,
    ICrudService<TUnitOfWork, TKey, TEntity, TModel, TPagingInput, TPagingOutput, TCreateInput, TUpdateInput>
    where TUnitOfWork : IUnitOfWork
    where TEntity : class, IEntity<TKey>, ILogCreatedEntity
    where TPagingInput : IPaging
    where TCreateInput : ICreateInput<TEntity, TCreateInput>
{
    private readonly TUnitOfWork _uow;
    private readonly IWorkContext _workContext;

    protected CrudBatchService(TUnitOfWork uow, IWorkContext workContext) : base(uow, workContext)
    {
        _uow = uow;
        _workContext = workContext;
    }


    // public async Task<IEnumerable<TModel>> CreateBatchAsync(IEnumerable<TCreateInput> input)
    // {
    //     await OnCreateBeginAsync(input);

    //     var repo = _uow.GetRepository<IRepository<TEntity>>();

    //     var entities = input.Select(x => MapToEntity(x));

    //     await OnBeforeSaveChangesInsertAsync(input, entities);
    //     await repo.InsertManyAsync(entities);
    //     var result = await _uow.CommitAsync();

    //     if (result > 0)
    //         await OnCreateSuccessAsync(input, entities);
    //     else
    //         await OnCreateFailAsync(input, entities);

    //     await OnCreateFinishAsync(input, entities);

    //     return entities.Select(x => MapToModel(x));
    // }

    // public async Task<IEnumerable<TModel>> UpdateBatchAsync(IEnumerable<TUpdateInput> input, bool asNoQuery = false)
    // {
    //     await Task.Yield();
    //     return null;
    //     // await OnUpdateBeginAsync(input);
    //     // var repo = _uow.GetRepository<IRepository<TEntity>>();

    //     // foreach (var item in input)
    //     // {
    //     //     TEntity entity = null;
    //     //     if (asNoQuery)
    //     //     {
    //     //         entity = Activator.CreateInstance<TEntity>();
    //     //         entity.Id = item.;
    //     //     }
    //     //     else
    //     //     {
    //     //         entity = await repo.GetQuery().QueryById(id).FirstOrDefaultAsync();
    //     //     }

    //     //     if (entity == null)
    //     //         return default(TModel);

    //     //     MapToEntity(input, entity);
    //     //     SetUpdateAudited(entity);
    //     // }


    //     // await OnBeforeSaveChangesUpdateAsync(input, entity);
    //     // var result = await _uow.CommitAsync();

    //     // if (result == 0)
    //     //     await OnUpdateFailAsync(id, input, entity);
    //     // else
    //     //     await OnUpdateSuccessAsync(id, input, entity);

    //     // await OnUpdateFinishAsync(id, input, entity);
    //     // return MapToModel(entity);
    // }
}