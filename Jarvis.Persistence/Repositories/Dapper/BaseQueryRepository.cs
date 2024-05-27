using System.Linq.Expressions;
using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Domain.Common.Interfaces;

namespace Jarvis.Persistence.Repositories.Dapper;

public class BaseQueryRepository<TEntity> : IQueryRepository<TEntity>
    where TEntity : class, IEntity
{
    private DapperDbContext StorageContext;

    public void SetStorageContext(IStorageContext storageContext)
    {
        StorageContext = storageContext as DapperDbContext;
    }

    public IQueryable<TEntity> GetQuery(bool asNoTracking = false)
    {
        throw new NotImplementedException();
    }

    public  Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate = null, bool asNoTracking = false)
    {
        throw new NotImplementedException();
    }

    public Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate = null, bool asNoTracking = false)
    {
        throw new NotImplementedException();
    }

    public Task<ICollection<TEntity>> ListAsync(Expression<Func<TEntity, bool>> predicate = null, bool asNoTracking = false)
    {
        throw new NotImplementedException();
    }
}