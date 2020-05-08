// using Infrastructure.Database.Abstractions;
// using Infrastructure.Database.Constants;
// using Infrastructure.Database.Models;
// using Microsoft.AspNetCore.Authorization;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;

// namespace Jarvis.Core.Abstractions
// {
//     public interface ICrudService<TModel, TEntity, TKey> where TEntity : CrudEntity<TKey>
//     {
//         IPaged<TModel> Query(IPaging paging);

//         TModel Query(TKey id);

//         void Create(TModel model);

//         void Update(TKey id, TModel model);

//         void Delete(TKey id);
//     }

//     //public abstract class CrudService<TModel, TEntity, TKey> : ICrudService<TModel, TEntity, TKey> where TEntity : CrudEntity<TKey>
//     //{
//     //    private readonly IUnitOfWork _unitOfWork;
//     //    private readonly IWorkContext _workContext;

//     //    public CrudService(IUnitOfWork unitOfWork, IWorkContext workContext)
//     //    {
//     //        _unitOfWork = unitOfWork;
//     //        _workContext = workContext;
//     //    }

//     //    public async Task<IPaged<TModel>> QueryAsync(IPaging paging, TModel model)
//     //    {
//     //        var session = await _workContext.GetSessionAsync();
//     //        var policy = $"{typeof(TEntity).Name}_Read";
//     //        var claim = session.Claims[policy];

//     //        var repo = _unitOfWork.GetRepository<TEntity>();
//     //        var paged = repo.Paging(
//     //            paging: paging,
//     //            filter: items => Filter(items, paging, model, session, claim),
//     //            include: items => Include(items),
//     //            order: items => Order(items));

//     //        var result = new Paged<TModel>
//     //        {
//     //            Data = paged.Data.Select(x => EntityToModel(x)),
//     //            Page = paged.Page,
//     //            Q = paged.Q,
//     //            Size = paged.Size,
//     //            TotalItems = paged.TotalItems,
//     //            TotalPages = paged.TotalPages
//     //        };
//     //        return result;
//     //    }

//     //    public async Task<TModel> QueryAsync(TKey id)
//     //    {
//     //        var session = await _workContext.GetSessionAsync();
//     //        var policy = $"{typeof(TEntity).Name}_Read";
//     //        var claim = session.Claims[policy];

//     //        var repo = _unitOfWork.GetRepository<TEntity>();
//     //        var entity = repo.First(items => SearchByPermission(items, session, claim).Where(x => x.Id.Equals(id)));
//     //        if (entity == null)
//     //            return default(TModel);

//     //        var model = EntityToModel(entity);
//     //        return model;
//     //    }

//     //    /// <summary>
//     //    /// Hàm sắp xếp, mặc định theo Id
//     //    /// </summary>
//     //    /// <param name="items"></param>
//     //    /// <returns></returns>
//     //    public virtual IOrderedQueryable<TEntity> Order(IQueryable<TEntity> items)
//     //    {
//     //        return items.OrderBy(x => x.Id);
//     //    }

//     //    /// <summary>
//     //    /// Hàm truy vấn các dữ liệu liên quan
//     //    /// </summary>
//     //    /// <param name="items"></param>
//     //    /// <returns></returns>
//     //    public virtual IQueryable<TEntity> Include(IQueryable<TEntity> items)
//     //    {
//     //        return items;
//     //    }

//     //    /// <summary>
//     //    /// Hàm truy vấn dữ liệu
//     //    /// </summary>
//     //    /// <param name="paging"></param>
//     //    /// <param name="model"></param>
//     //    /// <param name="session"></param>
//     //    /// <param name="claim"></param>
//     //    /// <param name="items"></param>
//     //    /// <returns></returns>
//     //    public virtual IQueryable<TEntity> Filter(IQueryable<TEntity> items, IPaging paging, TModel model, SessionModel session, string claim)
//     //    {
//     //        items = items.Where(x => x.CrudStatus != CrudStatus.Deleted);
//     //        items = SearchByPermission(items, session, claim);
//     //        if (string.IsNullOrWhiteSpace(paging.Q))
//     //            return AdvanceSearch(items, model);
//     //        else
//     //            return NormalSearch(items, model);
//     //    }

//     //    public virtual IQueryable<TEntity> NormalSearch(IQueryable<TEntity> items, TModel model)
//     //    {
//     //        return items;
//     //    }

//     //    public virtual IQueryable<TEntity> AdvanceSearch(IQueryable<TEntity> items, TModel model)
//     //    {
//     //        return items;
//     //    }

//     //    /// <summary>
//     //    /// Hàm truy vẫn theo quyền dữ liệu
//     //    /// </summary>
//     //    /// <param name="items"></param>
//     //    /// <param name="session"></param>
//     //    /// <param name="claim"></param>
//     //    /// <returns></returns>
//     //    public virtual IQueryable<TEntity> SearchByPermission(IQueryable<TEntity> items, SessionModel session, string claim)
//     //    {
//     //        var subContexts = session.SubContext.Select(x => x.Code).ToList();
//     //        switch (claim)
//     //        {
//     //            case "Mine_InCompany":
//     //                items = items.Where(x => x.Owner == session.Context.Code && x.CreatedBy == session.IdUser);
//     //                break;
//     //            case "Everybody_InCompany":
//     //                items = items.Where(x => x.Owner == session.Context.Code);
//     //                break;
//     //            case "Mine_InBranch":
//     //                items = items.Where(x => subContexts.Contains(x.Owner) && x.CreatedBy == session.IdUser);
//     //                break;
//     //            case "Everybody_InBranch":
//     //                items = items.Where(x => subContexts.Contains(x.Owner));
//     //                break;
//     //            default:
//     //                throw new Exception(ErrorCodes.ErrorDefault.QuyenDuLieuKhongDung.GetHashCode().ToString());
//     //        }

//     //        return items;
//     //    }



//     //    public async Task CreateAsync(TModel model)
//     //    {
//     //        var session = await _workContext.GetSessionAsync();
//     //        var policy = $"{typeof(TEntity).Name}_Create";
//     //        var claim = session.Claims[policy];

//     //        switch (claim)
//     //        {
//     //            case "Mine_InCompany":
//     //                if (session.Context.Code != session.Owner)
//     //                    throw new Exception(ErrorCodes.ErrorDefault.KhongCoQuyenDuLieu.GetHashCode().ToString());
//     //                break;
//     //            case "Mine_InBranch":
//     //                if (!session.SubContext.Any(x => x.Code == session.Owner))
//     //                    throw new Exception(ErrorCodes.ErrorDefault.KhongCoQuyenDuLieu.GetHashCode().ToString());
//     //                break;
//     //            default:
//     //                throw new Exception(ErrorCodes.ErrorDefault.QuyenDuLieuKhongDung.GetHashCode().ToString());
//     //        }

//     //        OnBeforeCreate(model);

//     //        TEntity entity = Activator.CreateInstance<TEntity>();
//     //        ModelToEntity(model, entity);
//     //        entity.Status = ActiveStatus.InActive.ToString();
//     //        entity.CreatedAt = DateTime.UtcNow;
//     //        entity.CreatedBy = session.IdUser;
//     //        entity.Owner = session.Context.Code;

//     //        var repo = _unitOfWork.GetRepository<TEntity>();
//     //        repo.Add(entity);
//     //        _unitOfWork.SaveChanges();

//     //        OnAfterCreate(model);
//     //    }

//     //    /// <summary>
//     //    /// Hàm xử lý trước khi tạo
//     //    /// </summary>
//     //    /// <param name="model"></param>
//     //    public virtual void OnBeforeCreate(TModel model)
//     //    {

//     //    }

//     //    /// <summary>
//     //    /// Hàm xử lý sau khi tạo
//     //    /// </summary>
//     //    /// <param name="model"></param>
//     //    public virtual void OnAfterCreate(TModel model)
//     //    {

//     //    }



//     //    public async Task UpdateAsync(TKey id, TModel model)
//     //    {
//     //        var session = await _workContext.GetSessionAsync();
//     //        var policy = $"{typeof(TEntity).Name}_Update";
//     //        var claim = session.Claims[policy];

//     //        OnBeforeUpdate(model);

//     //        var repo = _unitOfWork.GetRepository<TEntity>();
//     //        var entity = repo.First(items => SearchByPermission(items, session, claim).Where(x => x.Id.Equals(id)));
//     //        if (entity == null)
//     //            throw new Exception(ErrorCodes.ErrorDefault.DuLieuKhongTonTai.GetHashCode().ToString());

//     //        ModelToEntity(model, entity);
//     //        entity.UpdatedBy = session.IdUser;
//     //        entity.UpdatedAt = DateTime.UtcNow;

//     //        repo.Update(entity);
//     //        _unitOfWork.SaveChanges();

//     //        OnAfterUpdate(model);
//     //    }

//     //    /// <summary>
//     //    /// Hàm xử lý trước khi sửa
//     //    /// </summary>
//     //    /// <param name="model"></param>
//     //    public virtual void OnBeforeUpdate(TModel model)
//     //    {

//     //    }

//     //    /// <summary>
//     //    /// Hàm xử lý sau khi sửa
//     //    /// </summary>
//     //    /// <param name="model"></param>
//     //    public virtual void OnAfterUpdate(TModel model)
//     //    {

//     //    }



//     //    public async Task DeleteAsync(TKey id)
//     //    {
//     //        var session = await _workContext.GetSessionAsync();
//     //        var policy = $"{typeof(TEntity).Name}_Delete";
//     //        var claim = session.Claims[policy];

//     //        OnBeforeDelete(id);

//     //        var repo = _unitOfWork.GetRepository<TEntity>();
//     //        var entity = repo.First(items => SearchByPermission(items, session, claim).Where(x => x.Id.Equals(id)));
//     //        if (entity == null)
//     //            throw new Exception(ErrorCodes.ErrorDefault.DuLieuKhongTonTai.GetHashCode().ToString());

//     //        entity.Status = ActiveStatus.Deleted.ToString();
//     //        entity.DeletedAt = DateTime.UtcNow;
//     //        entity.DeletedBy = session.IdUser;
//     //        repo.Update(entity);
//     //        _unitOfWork.SaveChanges();

//     //        OnAfterDelete(id);
//     //    }

//     //    /// <summary>
//     //    /// Hàm xử lý trước khi xóa
//     //    /// </summary>
//     //    /// <param name="model"></param>
//     //    public virtual void OnBeforeDelete(TKey id)
//     //    {

//     //    }

//     //    /// <summary>
//     //    /// Hàm xử lý sau khi xóa
//     //    /// </summary>
//     //    /// <param name="model"></param>
//     //    public virtual void OnAfterDelete(TKey id)
//     //    {

//     //    }



//     //    /// <summary>
//     //    /// Hàm chuyển từ Model sang Entity
//     //    /// </summary>
//     //    /// <param name="model"></param>
//     //    /// <returns></returns>
//     //    public abstract void ModelToEntity(TModel model, TEntity entity);

//     //    /// <summary>
//     //    /// Hàm chuyển từ Entity sang Model
//     //    /// </summary>
//     //    /// <param name="entity"></param>
//     //    /// <returns></returns>
//     //    public abstract TModel EntityToModel(TEntity entity);
//     //}
// }
