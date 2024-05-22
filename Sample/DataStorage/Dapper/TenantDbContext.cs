using System.Data;
using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Persistence.Repositories.Dapper;

namespace Sample.DataStorage.Dapper;

public class TenantDbContext : DapperDbContext, IStorageContext
{
    public TenantDbContext(
        IDbConnection connection) : base(connection)
    {
    }
}