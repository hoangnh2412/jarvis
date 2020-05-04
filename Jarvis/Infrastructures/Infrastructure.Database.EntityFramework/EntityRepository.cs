using Infrastructure.Database.Abstractions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Infrastructure.Database.EntityFramework
{
    public class EntityRepository<TEntity> : IRepository<TEntity> where TEntity : class, IEntity
    {
        protected DbContext StorageContext { get; private set; }
        protected DbSet<TEntity> DbSet { get; private set; }
        protected IQueryable<TEntity> Query { get; private set; }

        public void SetStorageContext(IStorageContext storageContext)
        {
            StorageContext = storageContext as DbContext;
            DbSet = StorageContext.Set<TEntity>();
            Query = DbSet as IQueryable<TEntity>;
        }

        public IStorageContext GetStorageContext()
        {
            return StorageContext as IStorageContext;
        }

        public IQueryable<TEntity> GetQuery()
        {
            return Query;
        }

        public async Task<List<TEntity>> GetAllAsync()
        {
            return await Query.ToListAsync();
        }

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate = null)
        {
            if (predicate == null)
                return await Query.CountAsync();

            return await Query.CountAsync(predicate);
        }

        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate = null)
        {
            if (predicate == null)
                return await Query.AnyAsync();

            return await Query.AnyAsync(predicate);
        }

        public async Task<bool> InsertAsync(TEntity entity)
        {
            await DbSet.AddAsync(entity);
            return true;
        }

        public async Task InsertsAsync(IEnumerable<TEntity> entities)
        {
            await DbSet.AddRangeAsync(entities);
        }

        public void UpdateFields(TEntity entity, params Expression<Func<TEntity, object>>[] properties)
        {
            var entry = StorageContext.Entry(entity);
            if (entry.State == EntityState.Detached)
                StorageContext.Attach(entity);

            foreach (var property in properties)
            {
                entry.Property(property).IsModified = true;
            }
        }

        public void UpdateFields(TEntity entity, params KeyValuePair<Expression<Func<TEntity, object>>, object>[] properties)
        {
            var entry = StorageContext.Entry(entity);
            if (entry.State == EntityState.Detached)
                StorageContext.Attach(entity);

            foreach (var property in properties)
            {
                entry.Property(property.Key).CurrentValue = property.Value;
                entry.Property(property.Key).IsModified = true;
            }
        }

        public bool Update(TEntity entity)
        {
            // DbSet.Update(entity);

            var entry = StorageContext.Entry(entity);
            if (entry.State == EntityState.Detached)
                StorageContext.Attach(entity);

            entry.State = EntityState.Modified;
            return true;
        }

        public void Updates(IEnumerable<TEntity> entities)
        {
            // DbSet.UpdateRange(entities);

            foreach (var entity in entities)
            {
                var entry = StorageContext.Entry(entity);
                if (entry.State == EntityState.Detached)
                    StorageContext.Attach(entity);

                entry.State = EntityState.Modified;
            }
        }

        public bool Delete(TEntity entity)
        {
            // DbSet.Remove(entity);

            var entry = StorageContext.Entry(entity);
            if (entry.State == EntityState.Detached)
                StorageContext.Attach(entity);

            entry.State = EntityState.Deleted;
            // StorageContext.Remove(entity);
            return true;
        }

        public void Deletes(IEnumerable<TEntity> entities)
        {
            // StorageContext.RemoveRange(entities);

            foreach (var entity in entities)
            {
                var entry = StorageContext.Entry(entity);
                if (entry.State == EntityState.Detached)
                    StorageContext.Attach(entity);

                entry.State = EntityState.Deleted;
            }
        }
    }
}
