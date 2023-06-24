using System;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Database.Abstractions;
using Infrastructure.Database.EntityFramework;
using Infrastructure.Database.Models;
using Jarvis.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace Jarvis.Core.Abstractions
{
    public abstract class CrudService<TUnitOfWork, TKey, TEntity, TModel, TPagingOutput, TCreateInput, TUpdateInput> : ICrudService<TUnitOfWork, TKey, TEntity, TModel, TPagingOutput, TCreateInput, TUpdateInput>
        where TUnitOfWork : IUnitOfWork
        where TEntity : class, IEntity<TKey>, ITenantEntity, ILogCreatedEntity, ILogUpdatedEntity, ILogDeletedEntity, ILogDeletedVersionEntity<TKey>
    {
        private readonly TUnitOfWork _uow;
        private readonly IDomainWorkContext _workContext;

        public CrudService(TUnitOfWork uow, IDomainWorkContext workContext)
        {
            _uow = uow;
            _workContext = workContext;
        }

        public virtual async Task<IPaged<TPagingOutput>> PaginationAsync(IPaging paging)
        {
            var repo = _uow.GetRepository<IRepository<TEntity>>();
            var queryable = repo.GetQuery();
            queryable = PaginationWhere(queryable, paging);
            queryable = PaginationOrderBy(queryable, paging);
            var paged = await queryable.ToPaginationAsync(paging);
            return new Paged<TPagingOutput>
            {
                Data = paged.Data.Select(entity => MapToPagingOutput(entity)),
                Page = paged.Page,
                Q = paged.Q,
                Size = paged.Size,
                TotalItems = paged.TotalItems,
                TotalPages = paged.TotalPages
            };
        }

        public virtual async Task<TModel> GetByIdAsync(TKey id)
        {
            var repo = _uow.GetRepository<IRepository<TEntity>>();
            var entity = await repo.GetQuery().FirstOrDefaultAsync(x => x.Id.Equals(id));
            return MapToModel(entity);
        }

        public virtual async Task<TModel> GetByKeyAsync(Guid key)
        {
            var repo = _uow.GetRepository<IRepository<TEntity>>();
            var entity = await repo.GetQuery().FirstOrDefaultAsync(x => x.Key.Equals(key));
            return MapToModel(entity);
        }

        public virtual async Task<int> CreateAsync(TCreateInput input)
        {
            await OnCreateBeginAsync();

            var repo = _uow.GetRepository<IRepository<TEntity>>();
            var entity = MapToEntity(input);
            SetTenantCode(entity);

            entity.Key = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.CreatedAtUtc = DateTime.UtcNow;
            entity.CreatedBy = _workContext.GetUserKey();

            await repo.InsertAsync(entity);
            var result = await _uow.CommitAsync();

            if (result > 0)
                await OnCreateSuccessAsync();
            else
                await OnCreateFailAsync();

            await OnCreateEndAsync();
            return result;
        }

        public virtual Task OnCreateEndAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task OnCreateFailAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task OnCreateSuccessAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task OnCreateBeginAsync()
        {
            return Task.CompletedTask;
        }

        public virtual async Task<int> UpdateAsync(TKey id, TUpdateInput input)
        {
            await OnUpdateBeginAsync();

            var repo = _uow.GetRepository<IRepository<TEntity>>();
            var entity = await repo.GetQuery().FirstOrDefaultAsync(x => x.Id.Equals(id));
            if (entity == null)
                return 0;

            MapToEntity(input, entity);

            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedAtUtc = DateTime.UtcNow;
            entity.UpdatedBy = _workContext.GetUserKey();

            var result = await _uow.CommitAsync();

            if (result > 0)
                await OnUpdateFailAsync();
            else
                await OnUpdateSuccessAsync();

            await OnUpdateFinishAsync();
            return result;
        }

        public virtual async Task<int> UpdateAsync(Guid key, TUpdateInput input)
        {
            await OnUpdateBeginAsync();

            var repo = _uow.GetRepository<IRepository<TEntity>>();
            var entity = await repo.GetQuery().FirstOrDefaultAsync(x => x.Key.Equals(key));
            if (entity == null)
                return 0;

            MapToEntity(input, entity);

            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedAtUtc = DateTime.UtcNow;
            entity.UpdatedBy = _workContext.GetUserKey();

            var result = await _uow.CommitAsync();

            if (result > 0)
                await OnUpdateFailAsync();
            else
                await OnUpdateSuccessAsync();

            await OnUpdateFinishAsync();
            return result;
        }

        public virtual Task OnUpdateFinishAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task OnUpdateSuccessAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task OnUpdateFailAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task OnUpdateBeginAsync()
        {
            return Task.CompletedTask;
        }

        public virtual async Task<int> DeleteAsync(TKey id)
        {
            await OnDeleteBeginAsync();
            var repo = _uow.GetRepository<IRepository<TEntity>>();
            var entity = await repo.GetQuery().FirstOrDefaultAsync(x => x.Id.Equals(id));
            if (entity == null)
                return 0;

            repo.Delete(entity);
            var result = await _uow.CommitAsync();

            if (result > 0)
                await OnDeleteSuccessAsync();
            else
                await OnDeleteFailAsync();

            await OnDeleteFinishAsync();
            return result;
        }

        public virtual async Task<int> DeleteAsync(Guid key)
        {
            await OnDeleteBeginAsync();
            var repo = _uow.GetRepository<IRepository<TEntity>>();
            var entity = await repo.GetQuery().FirstOrDefaultAsync(x => x.Key.Equals(key));
            if (entity == null)
                return 0;

            repo.Delete(entity);
            var result = await _uow.CommitAsync();

            if (result > 0)
                await OnDeleteSuccessAsync();
            else
                await OnDeleteFailAsync();

            await OnDeleteFinishAsync();
            return result;
        }

        public virtual Task OnDeleteFinishAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task OnDeleteFailAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task OnDeleteSuccessAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task OnDeleteBeginAsync()
        {
            return Task.CompletedTask;
        }


        public virtual async Task<int> TrashAsync(TKey id)
        {
            await OnTrashBeginAsync();

            var repo = _uow.GetRepository<IRepository<TEntity>>();
            var entity = await repo.GetQuery().FirstOrDefaultAsync(x => x.Id.Equals(id));
            if (entity == null)
                return 0;

            entity.DeletedAt = DateTime.UtcNow;
            entity.DeletedAtUtc = DateTime.UtcNow;
            entity.DeletedVersion = entity.Id;
            entity.DeletedBy = _workContext.GetUserKey();

            var result = await _uow.CommitAsync();
            if (result > 0)
                await OnTrashSuccessAsync();
            else
                await OnTrashFailAsync();

            await OnTrashFinishAsync();
            return result;
        }

        public virtual async Task<int> TrashAsync(Guid key)
        {
            await OnTrashBeginAsync();

            var repo = _uow.GetRepository<IRepository<TEntity>>();
            var entity = await repo.GetQuery().FirstOrDefaultAsync(x => x.Key.Equals(key));
            if (entity == null)
                return 0;

            entity.DeletedAt = DateTime.UtcNow;
            entity.DeletedAtUtc = DateTime.UtcNow;
            entity.DeletedVersion = entity.Id;
            entity.DeletedBy = _workContext.GetUserKey();

            var result = await _uow.CommitAsync();
            if (result > 0)
                await OnTrashSuccessAsync();
            else
                await OnTrashFailAsync();

            await OnTrashFinishAsync();
            return result;
        }

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

        public virtual void SetTenantCode(TEntity entity)
        {
            if (!(entity is ITenantEntity) || entity.GetType().GetProperty(nameof(ITenantEntity.TenantCode)) == null)
                return;

            var tenantKey = _workContext.GetTenantKey();
            if (tenantKey == null)
                return;

            var property = entity.GetType().GetProperty(nameof(ITenantEntity.TenantCode));
            if (property == null)
                return;

            property.SetValue(entity, tenantKey);
        }

        public virtual IQueryable<TEntity> PaginationOrderBy(IQueryable<TEntity> queryable, IPaging paging)
        {
            return queryable.OrderByDescending(x => x.CreatedAt);
        }

        public virtual IQueryable<TEntity> PaginationWhere(IQueryable<TEntity> queryable, IPaging paging)
        {
            return queryable.Where(x => x.DeletedVersion.Equals(default(TKey)));
        }

        protected abstract TPagingOutput MapToPagingOutput(TEntity entity);
        protected abstract TModel MapToModel(TEntity entity);
        protected abstract TEntity MapToEntity(TCreateInput input);
        protected abstract TEntity MapToEntity(TUpdateInput input, TEntity entity);
    }
}