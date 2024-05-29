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
public abstract partial class CrudService<TUnitOfWork, TKey, TEntity, TModel, TPagingInput, TPagingOutput, TCreateInput, TUpdateInput>
    : ICrudService<TUnitOfWork, TKey, TEntity, TModel, TPagingInput, TPagingOutput, TCreateInput, TUpdateInput>
    where TUnitOfWork : IUnitOfWork
    where TEntity : class, IEntity<TKey>, ILogCreatedEntity
    where TPagingInput : IPaging
    where TCreateInput : ICreateInput<TEntity, TCreateInput>
{
    private readonly TUnitOfWork _uow;
    private readonly IWorkContext _workContext;

    public CrudService(
        TUnitOfWork uow,
        IWorkContext workContext)
    {
        _uow = uow;
        _workContext = workContext;
    }

    /// <summary>
    /// Return list of items after pagination
    /// </summary>
    /// <param name="paging">Search parameters</param>
    /// <param name="asNoTracking">If asNoTracking is true then DbContext not tracking change of entity</param>
    /// <returns></returns>
    public virtual async Task<IPaged<TPagingOutput>> PaginationAsync(TPagingInput paging, bool asNoTracking = false)
    {
        var repo = _uow.GetRepository<IEFRepository<TEntity>>();
        var queryable = repo.GetQuery(asNoTracking);
        queryable = PaginationCondition(queryable, paging);
        queryable = PaginationSearch(queryable, paging);
        queryable = queryable.QueryByTenantId(_workContext.TenantId);
        queryable = PaginationOrderBy(queryable, paging);
        queryable = PaginationSelect(queryable, paging);
        var paged = await queryable.ToPagedAsync(paging);
        return new Paged<TPagingOutput>
        {
            Data = paged.Data.Select(entity => MapToPagingOutput(entity)),
            Page = paged.Page,
            Query = paged.Query,
            Size = paged.Size,
            TotalItems = paged.TotalItems,
            TotalPages = paged.TotalPages
        };
    }

    public virtual IQueryable<TEntity> PaginationSelect(IQueryable<TEntity> queryable, TPagingInput paging)
    {
        return queryable;
    }

    /// <summary>
    /// Return a item search by Id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="asNoTracking">If asNoTracking is true then DbContext not tracking change of entity</param>
    /// <returns></returns>
    public virtual async Task<TModel> GetByIdAsync(TKey id, bool asNoTracking = false)
    {
        var repo = _uow.GetRepository<IEFRepository<TEntity>>();
        var entity = await repo.GetQuery(asNoTracking)
            .QueryById(id)
            .QueryByTenantId(_workContext.TenantId)
            .FirstOrDefaultAsync();

        if (entity == null)
            return default(TModel);

        return MapToModel(entity);
    }

    /// <summary>
    /// Return a item search by Key
    /// </summary>
    /// <param name="key"></param>
    /// <param name="asNoTracking">If asNoTracking is true then DbContext not tracking change of entity</param>
    /// <returns></returns>
    // public virtual async Task<TModel> GetByKeyAsync(Guid key, bool asNoTracking = false)
    // {
    //     var repo = _uow.GetRepository<IRepository<TEntity>>();
    //     var entity = await repo.GetQuery(asNoTracking)
    //         .FirstOrDefaultAsync(x => x.Key.Equals(key));

    //     if (entity == null)
    //         return default(TModel);

    //     return MapToModel(entity);
    // }

    /// <summary>
    /// Return a item search by Code
    /// </summary>
    /// <param name="code"></param>
    /// <param name="asNoTracking">If asNoTracking is true then DbContext not tracking change of entity</param>
    /// <returns></returns>
    public virtual async Task<TModel> GetByCodeAsync(string code, bool asNoTracking = false)
    {
        var fieldCode = TypeExtension.GetPropertyByAttribute(typeof(TEntity), typeof(CodeAttribute));
        if (fieldCode == null)
            return default(TModel);

        var repo = _uow.GetRepository<IEFRepository<TEntity>>();
        var entity = await repo.GetQuery(asNoTracking)
            .QueryEqual(fieldCode.Name, code)
            .QueryByTenantId(_workContext.TenantId)
            .FirstOrDefaultAsync();

        if (entity == null)
            return default(TModel);

        return MapToModel(entity);
    }

    /// <summary>
    /// Create new item
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public virtual async Task<TModel> CreateAsync(TCreateInput input)
    {
        await OnCreateBeginAsync(input);

        var repo = _uow.GetRepository<IEFRepository<TEntity>>();

        var entity = MapToEntity(input);

        await OnBeforeSaveChangesInsertAsync(input, entity);
        await repo.InsertAsync(entity);
        var result = await _uow.CommitAsync();

        if (result > 0)
            await OnCreateSuccessAsync(input, entity);
        else
            await OnCreateFailAsync(input, entity);

        await OnCreateFinishAsync(input, entity);
        return MapToModel(entity);
    }

    /// <summary>
    /// Function handle before save
    /// </summary>
    /// <param name="input"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual Task OnBeforeSaveChangesInsertAsync(TCreateInput input, TEntity entity)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Function handle after save
    /// </summary>
    /// <param name="input"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual Task OnCreateFinishAsync(TCreateInput input, TEntity entity)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Function handle when save fail
    /// </summary>
    /// <param name="input"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual Task OnCreateFailAsync(TCreateInput input, TEntity entity)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Function handle when save success
    /// </summary>
    /// <param name="input"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual Task OnCreateSuccessAsync(TCreateInput input, TEntity entity)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Function handle when creating
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public virtual Task OnCreateBeginAsync(TCreateInput input)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Update a item by Id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    public virtual async Task<TModel> UpdateAsync(TKey id, TUpdateInput input, bool asNoQuery = false)
    {
        return await UpdateInternalAsync(id, input, null, asNoQuery);
    }

    public virtual async Task<TModel> UpdateAsync(TKey id, TUpdateInput input, Action<TUpdateInput, TEntity> mapping, bool asNoQuery = false)
    {
        return await UpdateInternalAsync(id, input, mapping, asNoQuery);
    }

    private async Task<TModel> UpdateInternalAsync(TKey id, TUpdateInput input, Action<TUpdateInput, TEntity> mapping, bool asNoQuery)
    {
        await OnUpdateBeginAsync(id, input);
        var repo = _uow.GetRepository<IEFRepository<TEntity>>();

        TEntity entity = null;
        if (asNoQuery)
        {
            entity = Activator.CreateInstance<TEntity>();
            entity.Id = id;
        }
        else
        {
            entity = await repo.GetQuery().QueryById(id).FirstOrDefaultAsync();
        }

        if (entity == null)
            return default(TModel);

        if (mapping == null)
            MapToEntity(input, entity);
        else
            mapping.Invoke(input, entity);

        SetUpdateAudited(entity);

        await OnBeforeSaveChangesUpdateAsync(input, entity);

        if (asNoQuery)
        {
            var dbContext = _uow.GetDbContext() as DbContext;
            var entry = dbContext.Entry(entity);
            if (entry.State == EntityState.Detached)
                dbContext.Attach(entity);

            entry.State = EntityState.Modified;
        }

        var result = await _uow.CommitAsync();

        if (result == 0)
            await OnUpdateFailAsync(id, input, entity);
        else
            await OnUpdateSuccessAsync(id, input, entity);

        await OnUpdateFinishAsync(id, input, entity);
        return MapToModel(entity);
    }

    /// <summary>
    /// Update a item by Key
    /// </summary>
    /// <param name="key"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    // public virtual async Task<TModel> UpdateByKeyAsync(Guid key, TUpdateInput input)
    // {
    //     await OnUpdateBeginAsync(key, input);

    //     var repo = _uow.GetRepository<IRepository<TEntity>>();
    //     var entity = await repo.GetQuery().FirstOrDefaultAsync(x => x.Key.Equals(key));
    //     if (entity == null)
    //         return default(TModel);

    //     MapToEntity(input, entity);
    //     SetUpdateAudited(entity);

    //     await OnBeforeSaveChangesUpdateAsync(input, entity);
    //     var result = await _uow.CommitAsync();

    //     if (result > 0)
    //         await OnUpdateFailAsync(key, input, entity);
    //     else
    //         await OnUpdateSuccessAsync(key, input, entity);

    //     await OnUpdateFinishAsync(key, input, entity);
    //     return MapToModel(entity);
    // }

    /// <summary>
    /// Function handle before save
    /// </summary>
    /// <param name="input"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual Task OnBeforeSaveChangesUpdateAsync(TUpdateInput input, TEntity entity)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Function handle after save
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <param name="entity"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public virtual Task OnUpdateFinishAsync<T>(T id, TUpdateInput input, TEntity entity)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Function handle when save success
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <param name="entity"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public virtual Task OnUpdateSuccessAsync<T>(T id, TUpdateInput input, TEntity entity)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Function handle when save fail
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <param name="entity"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public virtual Task OnUpdateFailAsync<T>(T id, TUpdateInput input, TEntity entity)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Function handle when updating
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public virtual Task OnUpdateBeginAsync<T>(T id, TUpdateInput input)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Delete a item by Id without query first
    /// </summary>
    /// <param name="id"></param>
    /// <param name="asNoQuery"></param>
    /// <returns></returns>
    public virtual async Task<TModel> DeleteByIdAsync(TKey id, bool asNoQuery = true)
    {
        await OnDeleteBeginAsync(id);
        var repo = _uow.GetRepository<IEFRepository<TEntity>>();

        TEntity entity = null;
        if (asNoQuery)
        {
            entity = Activator.CreateInstance<TEntity>();
            entity.Id = id;
        }
        else
        {
            entity = await repo.GetQuery().QueryById(id).FirstOrDefaultAsync();
        }

        if (entity == null)
            return default(TModel);

        await repo.DeleteAsync(entity);
        var result = await _uow.CommitAsync();

        if (result > 0)
            await OnDeleteSuccessAsync(entity);
        else
            await OnDeleteFailAsync(entity);

        await OnDeleteFinishAsync(entity);
        return MapToModel(entity);
    }

    /// <summary>
    /// Delete a item by Key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    // public virtual async Task<TModel> DeleteByKeyAsync(Guid key)
    // {
    //     await OnDeleteBeginAsync(key);
    //     var repo = _uow.GetRepository<IRepository<TEntity>>();
    //     var entity = await repo.GetQuery().FirstOrDefaultAsync(x => x.Key.Equals(key));
    //     if (entity == null)
    //         return default(TModel);

    //     await repo.DeleteAsync(entity);
    //     var result = await _uow.CommitAsync();

    //     if (result > 0)
    //         await OnDeleteSuccessAsync(entity);
    //     else
    //         await OnDeleteFailAsync(entity);

    //     await OnDeleteFinishAsync(entity);
    //     return MapToModel(entity);
    // }

    /// <summary>
    /// Function handle after save
    /// </summary>
    /// <param name="id"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public virtual Task OnDeleteFinishAsync(TEntity entity)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Function handle when save fail
    /// </summary>
    /// <param name="id"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public virtual Task OnDeleteFailAsync(TEntity entity)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Function handle when save success
    /// </summary>
    /// <param name="id"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public virtual Task OnDeleteSuccessAsync(TEntity entity)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Function handle when updating
    /// </summary>
    /// <param name="id"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public virtual Task OnDeleteBeginAsync<T>(T id)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Detect type of field Id form entity, if type is Guid, set new value
    /// </summary>
    /// <param name="entity"></param>
    public virtual void SetId(TEntity entity)
    {
        if (entity.Id != null)
            return;

        if (typeof(TKey) == typeof(Guid))
            entity.GetType().GetProperty(nameof(IEntity<TKey>.Id)).SetValue(entity, Guid.NewGuid());
    }

    /// <summary>
    /// Set TenantId get form work context to entity
    /// </summary>
    /// <param name="entity"></param>
    public virtual void SetTenantId(TEntity entity)
    {
        if (!(entity is ITenantEntity))
            return;

        var type = entity.GetType();

        var fieldTenantId = type.GetProperty(nameof(ITenantEntity.TenantId));
        if (fieldTenantId != null)
            fieldTenantId.SetValue(entity, _workContext.TenantId);
    }

    /// <summary>
    /// Set CreatedAt, CreatedBy from work context to entity
    /// </summary>
    /// <param name="entity"></param>
    public virtual void SetCreateAudited(TEntity entity)
    {
        if (!(entity is ILogCreatedEntity))
            return;

        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = _workContext.UserId;
    }

    /// <summary>
    /// Set UpdatedAt, UpdatedBy get form work context to entity
    /// </summary>
    /// <param name="entity"></param>
    public virtual void SetUpdateAudited(TEntity entity)
    {
        if (!(entity is ILogUpdatedEntity))
            return;

        var type = entity.GetType();

        var fieldUpdatedAt = type.GetProperty(nameof(ILogUpdatedEntity.UpdatedAt));
        if (fieldUpdatedAt != null)
            fieldUpdatedAt.SetValue(entity, DateTime.UtcNow);

        var fieldUpdatedBy = type.GetProperty(nameof(ILogUpdatedEntity.UpdatedBy));
        if (fieldUpdatedBy != null)
            fieldUpdatedBy.SetValue(entity, _workContext.UserId);
    }

    /// <summary>
    /// Set UpdatedAt, UpdatedBy get form work context to entity
    /// </summary>
    /// <param name="entity"></param>
    public virtual void SetDeleteAudited(TEntity entity)
    {
        if (!(entity is ILogDeletedEntity<TKey>))
            return;

        var type = entity.GetType();

        var fieldDeletedAt = type.GetProperty(nameof(ILogDeletedEntity<TKey>.DeletedAt));
        if (fieldDeletedAt != null)
            fieldDeletedAt.SetValue(entity, DateTime.UtcNow);

        var fieldDeletedBy = type.GetProperty(nameof(ILogDeletedEntity<TKey>.DeletedBy));
        if (fieldDeletedBy != null)
            fieldDeletedBy.SetValue(entity, _workContext.UserId);

        var fieldDeletedId = type.GetProperty(nameof(ILogDeletedEntity<TKey>.DeletedId));
        if (fieldDeletedId != null)
            fieldDeletedId.SetValue(entity, entity.Id);
    }

    /// <summary>
    /// Order for pagination. Default order by descending CreatedAt
    /// </summary>
    /// <param name="queryable"></param>
    /// <param name="paging"></param>
    /// <returns></returns>
    public virtual IQueryable<TEntity> PaginationOrderBy(IQueryable<TEntity> queryable, TPagingInput paging)
    {
        if (paging.Sort == null || paging.Sort.Count == 0)
            return queryable.OrderByDescending(x => x.CreatedAt);
        else
            return queryable.OrderBy(paging.Sort);
    }

    /// <summary>
    /// Condition for pagination. Default filter by fieldName and fieldValue
    /// </summary>
    /// <param name="queryable"></param>
    /// <param name="paging"></param>
    /// <returns></returns>
    public virtual IQueryable<TEntity> PaginationSearch(IQueryable<TEntity> queryable, TPagingInput paging)
    {
        if (paging.Filters == null || paging.Filters.Count == 0)
            return queryable;

        foreach (var item in paging.Filters)
        {
            queryable = queryable.QueryEqual(item.Key, item.Value);
        }

        return queryable;
    }

    /// <summary>
    /// Condition for pagination. Default filter by TenantId
    /// </summary>
    /// <param name="queryable"></param>
    /// <param name="paging"></param>
    /// <returns></returns>
    public virtual IQueryable<TEntity> PaginationCondition(IQueryable<TEntity> queryable, TPagingInput paging)
    {
        return queryable;
    }

    /// <summary>
    /// Map create model to entity
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    protected virtual TEntity MapToEntity(TCreateInput input)
    {
        var entity = (TEntity)typeof(TCreateInput)
            .GetMethod("MapToEntity", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Invoke(null, new object[] { input });

        SetId(entity);
        SetCreateAudited(entity);
        SetTenantId(entity);

        return entity;
    }

    /// <summary>
    /// Map update model to entity
    /// </summary>
    /// <param name="input"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    protected virtual TEntity MapToEntity(TUpdateInput input, TEntity entity)
    {
        return (TEntity)typeof(TUpdateInput)
            .GetMethod("MapToEntity", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Invoke(null, new object[] { input, entity });
    }

    /// <summary>
    /// Map entity to pagination model
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    protected virtual TPagingOutput MapToPagingOutput(TEntity entity)
    {
        return (TPagingOutput)typeof(TPagingOutput)
            .GetMethod("MapToModel", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Invoke(null, new object[] { entity });
    }

    /// <summary>
    /// Map entity to model
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    protected virtual TModel MapToModel(TEntity entity)
    {
        return (TModel)typeof(TModel)
            .GetMethod("MapToModel", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Invoke(null, new object[] { entity });
    }

}