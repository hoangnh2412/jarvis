using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Infrastructure.Database.Abstractions
{
    public interface IRepository
    {
        void SetStorageContext(IStorageContext storageContext);

        IStorageContext GetStorageContext();
    }

    public interface IRepository<TEntity> : IRepository where TEntity : class, IEntity
    {
        IQueryable<TEntity> GetQuery();

        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate = null);

        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate = null);

        Task<bool> InsertAsync(TEntity entity);

        Task InsertsAsync(IEnumerable<TEntity> entities);

        bool Update(TEntity entity);

        void Updates(IEnumerable<TEntity> entities);

        void UpdateFields(TEntity entity, params Expression<Func<TEntity, object>>[] properties);

        void UpdateFields(TEntity entity, params KeyValuePair<Expression<Func<TEntity, object>>, object>[] properties);

        bool Delete(TEntity entity);
        
        void Deletes(IEnumerable<TEntity> entities);
    }
}
