//using Infrastructure.Database.Abstractions;
//using Infrastructure.Database.Constants;
//using Infrastructure.Database.Models;
//using Jarvis.Core.Database;
//using Jarvis.Core.Multitenant;
//using Jarvis.Core.Services;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Jarvis.Core.Abstractions
//{
//    public abstract class CrudService<TModel, TEntity, TKey> : ICrudService<TModel, TEntity, TKey> where TEntity : CrudEntity<TKey>
//    {
//        private readonly IUnitOfWork<CoreDbContext> _uow;
//        private readonly IWorkContext _workContext;
//        private readonly ITenantService _tenantService;

//        public CrudService(
//            IUnitOfWork<CoreDbContext> unitOfWork,
//            IWorkContext workContext,
//            ITenantService tenantService)
//        {
//            _uow = unitOfWork;
//            _workContext = workContext;
//            _tenantService = tenantService;
//        }

//        public IPaged<TModel> Query(IPaging paging)
//        {
//            var idTenant = _tenantService.GetIdTenant();

//            var repo = _uow.GetRepository<TEntity>();
//            var query = repo.Paging(
//                paging: paging,
//                filter: items => Filter(items, paging, idTenant),
//                include: items => Include(items, paging),
//                order: items => Order(items, paging));

//            var paged = new Paged<TEntity>(query, paging);
//            var result = new Paged<TModel>
//            {
//                Data = paged.Data.Select(x => EntityToModel(x)),
//                Page = paged.Page,
//                Q = paged.Q,
//                Size = paged.Size,
//                TotalItems = paged.TotalItems,
//                TotalPages = paged.TotalPages
//            };
//            return result;
//        }

//        public TModel Query(TKey id)
//        {
//            var repo = _uow.GetRepository<TEntity>();
//            var entity = repo.First(x => x.Id.Equals(id));
//            if (entity == null)
//                return default(TModel);

//            var model = EntityToModel(entity);
//            return model;
//        }

//        public void Create(TModel model)
//        {
//            OnBeforeCreate(model);

//            TEntity entity = Activator.CreateInstance<TEntity>();
//            ModelToEntity(model, entity);
//            entity.CreatedAtUtc = DateTime.UtcNow;
//            entity.CreatedAt = DateTime.Now;
//            entity.CreatedBy = _workContext.GetIdUser();
//            entity.IdTenant = _tenantService.GetIdTenant();

//            var repo = _uow.GetRepository<TEntity>();
//            repo.Add(entity);
//            _uow.Commit();

//            OnAfterCreate(model);
//        }

//        public void Update(TKey id, TModel model)
//        {
//            OnBeforeUpdate(model);
//            var idTenant = _tenantService.GetIdTenant();

//            var repo = _uow.GetRepository<TEntity>();
//            var entity = repo.First(x => x.Id.Equals(id) && x.IdTenant == idTenant);

//            ModelToEntity(model, entity);
//            entity.UpdatedBy = _workContext.GetIdUser();
//            entity.UpdatedAtUtc = DateTime.UtcNow;
//            entity.UpdatedAt = DateTime.Now;

//            repo.Update(entity);
//            _uow.Commit();

//            OnAfterUpdate(model);
//        }

//        public void Delete(TKey id)
//        {
//            OnBeforeDelete(id);
//            var idTenant = _tenantService.GetIdTenant();

//            var repo = _uow.GetRepository<TEntity>();
//            var entity = repo.First(x => x.Id.Equals(id) && x.IdTenant == idTenant);
//            //if (entity == null)
//            //    throw new Exception(ErrorCodes.ErrorDefault.DuLieuKhongTonTai.GetHashCode().ToString());

//            entity.DeletedAtUtc = DateTime.UtcNow;
//            entity.DeletedAt = DateTime.Now;
//            entity.DeletedBy = _workContext.GetIdUser();
//            repo.Update(entity);
//            _uow.Commit();

//            OnAfterDelete(id);
//        }




//        /// <summary>
//        /// Hàm sắp xếp, mặc định theo Id
//        /// </summary>
//        /// <param name="items"></param>
//        /// <returns></returns>
//        public virtual IOrderedQueryable<TEntity> Order(IQueryable<TEntity> items, IPaging paging)
//        {
//            return items.OrderBy(x => x.Id);
//        }

//        /// <summary>
//        /// Hàm truy vấn các dữ liệu liên quan
//        /// </summary>
//        /// <param name="items"></param>
//        /// <returns></returns>
//        public virtual IQueryable<TEntity> Include(IQueryable<TEntity> items, IPaging paging)
//        {
//            return items;
//        }

//        /// <summary>
//        /// Hàm truy vấn dữ liệu
//        /// </summary>
//        /// <param name="items"></param>
//        /// <param name="paging"></param>
//        /// <param name="idTenant"></param>
//        /// <returns></returns>
//        public virtual IQueryable<TEntity> Filter(IQueryable<TEntity> items, IPaging paging, Guid idTenant)
//        {
//            items = items.Where(x => !x.DeletedBy.HasValue);
//            //items = SearchByPermission(items, session, claim);
//            if (string.IsNullOrWhiteSpace(paging.Q))
//                return AdvanceSearch(items, paging);
//            else
//                return NormalSearch(items, paging);
//        }

//        public virtual IQueryable<TEntity> NormalSearch(IQueryable<TEntity> items, IPaging paging)
//        {
//            return items;
//        }

//        public virtual IQueryable<TEntity> AdvanceSearch(IQueryable<TEntity> items, IPaging paging)
//        {
//            return items;
//        }

//        /// <summary>
//        /// Hàm chuyển từ Model sang Entity
//        /// </summary>
//        /// <param name="model"></param>
//        /// <returns></returns>
//        public abstract void ModelToEntity(TModel model, TEntity entity);

//        /// <summary>
//        /// Hàm chuyển từ Entity sang Model
//        /// </summary>
//        /// <param name="entity"></param>
//        /// <returns></returns>
//        public abstract TModel EntityToModel(TEntity entity);

//        /// <summary>
//        /// Hàm xử lý trước khi tạo
//        /// </summary>
//        /// <param name="model"></param>
//        public virtual void OnBeforeCreate(TModel model)
//        {

//        }

//        /// <summary>
//        /// Hàm xử lý sau khi tạo
//        /// </summary>
//        /// <param name="model"></param>
//        public virtual void OnAfterCreate(TModel model)
//        {

//        }

//        /// <summary>
//        /// Hàm xử lý trước khi sửa
//        /// </summary>
//        /// <param name="model"></param>
//        public virtual void OnBeforeUpdate(TModel model)
//        {

//        }

//        /// <summary>
//        /// Hàm xử lý sau khi sửa
//        /// </summary>
//        /// <param name="model"></param>
//        public virtual void OnAfterUpdate(TModel model)
//        {

//        }

//        /// <summary>
//        /// Hàm xử lý trước khi xóa
//        /// </summary>
//        /// <param name="model"></param>
//        public virtual void OnBeforeDelete(TKey id)
//        {

//        }

//        /// <summary>
//        /// Hàm xử lý sau khi xóa
//        /// </summary>
//        /// <param name="model"></param>
//        public virtual void OnAfterDelete(TKey id)
//        {

//        }
//    }
//}
