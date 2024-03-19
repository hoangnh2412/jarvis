using Microsoft.EntityFrameworkCore;
using Jarvis.Application.DTOs;
using Jarvis.Application.Interfaces;
using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Domain.Common.Interfaces;

namespace Jarvis.Application.CrudGeneric;

public abstract class CrudFullAuditableService<TUnitOfWork, TKey, TEntity, TModel, TPagingInput, TPagingOutput, TCreateInput, TUpdateInput>
    : CrudService<TUnitOfWork, TKey, TEntity, TModel, TPagingInput, TPagingOutput, TCreateInput, TUpdateInput>,
    ICrudFullAuditableService<TUnitOfWork, TKey, TEntity, TModel, TPagingInput, TPagingOutput, TCreateInput, TUpdateInput>
    where TUnitOfWork : IUnitOfWork
    where TEntity : class, IEntity<TKey>, ILogCreatedEntity, ILogUpdatedEntity, ILogDeletedEntity<TKey>
    where TPagingInput : IPaging
    where TCreateInput : ICreateInput<TEntity, TCreateInput>
{
    private readonly TUnitOfWork _uow;
    private readonly IWorkContext _workContext;

    public CrudFullAuditableService(
        TUnitOfWork uow,
        IWorkContext workContext)
        : base(uow, workContext)
    {
        _uow = uow;
        _workContext = workContext;
    }

    public override async Task<TModel> GetByIdAsync(TKey id, bool asNoTracking = false)
    {
        var repo = _uow.GetRepository<IRepository<TEntity>>();
        var entity = await repo.GetQuery(asNoTracking)
            .FirstOrDefaultAsync(x => x.Id.Equals(id) && x.DeletedId.Equals(default(TKey)));
        return MapToModel(entity);
    }

    // public override async Task<TModel> GetByKeyAsync(Guid key, bool asNoTracking = false)
    // {
    //     var repo = _uow.GetRepository<IRepository<TEntity>>();
    //     var entity = await repo.GetQuery(asNoTracking)
    //         .FirstOrDefaultAsync(x => x.Key.Equals(key) && x.DeletedId.Equals(default(TKey)));
    //     return MapToModel(entity);
    // }

    public virtual async Task<int> TrashByIdAsync(TKey id)
    {
        await OnTrashBeginAsync();

        var repo = _uow.GetRepository<IRepository<TEntity>>();
        var entity = await repo.GetQuery().FirstOrDefaultAsync(x => x.Id.Equals(id));
        if (entity == null)
            return 0;

        SetDeleteAudited(entity);

        var result = await _uow.CommitAsync();
        if (result > 0)
            await OnTrashSuccessAsync();
        else
            await OnTrashFailAsync();

        await OnTrashFinishAsync();
        return result;
    }

    // public virtual async Task<int> TrashByKeyAsync(Guid key)
    // {
    //     await OnTrashBeginAsync();

    //     var repo = _uow.GetRepository<IRepository<TEntity>>();
    //     var entity = await repo.GetQuery().FirstOrDefaultAsync(x => x.Key.Equals(key));
    //     if (entity == null)
    //         return 0;

    //     SetDeleteAudited(entity);

    //     var result = await _uow.CommitAsync();
    //     if (result > 0)
    //         await OnTrashSuccessAsync();
    //     else
    //         await OnTrashFailAsync();

    //     await OnTrashFinishAsync();
    //     return result;
    // }

    public virtual Task OnTrashFinishAsync()
    {
        return Task.CompletedTask;
    }

    public virtual Task OnTrashFailAsync()
    {
        return Task.CompletedTask;
    }

    public virtual Task OnTrashSuccessAsync()
    {
        return Task.CompletedTask;
    }

    public virtual Task OnTrashBeginAsync()
    {
        return Task.CompletedTask;
    }

    public override IQueryable<TEntity> PaginationCondition(IQueryable<TEntity> queryable, TPagingInput paging)
    {
        queryable = base.PaginationCondition(queryable, paging);
        return queryable.Where(x => x.DeletedId.Equals(default(TKey)));
    }
}