using System.Data;
using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Domain.Common.Interfaces;

namespace Jarvis.Persistence.Repositories.Dapper;

public class BaseRepository<TEntity> : IRepository
    where TEntity : class, IEntity
{
    protected DapperDbContext StorageContext;
    private IDbConnection _connection;

    public void SetStorageContext(IStorageContext storageContext)
    {
        StorageContext = storageContext as DapperDbContext;
        _connection = StorageContext.Connection;
    }
}