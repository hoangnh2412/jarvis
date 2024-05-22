using System.Data;
using Jarvis.Application.Interfaces.Repositories;

namespace Jarvis.Persistence.Repositories.Dapper;

public class DapperDbContext : IStorageContext, IDisposable
{
    public IDbConnection Connection;

    public DapperDbContext(IDbConnection connection)
    {
        Connection = connection;
    }

    public void SetConnectionString(string connectionString)
    {
        Connection.ConnectionString = connectionString;
    }

    public string GetConnectionString()
    {
        return Connection.ConnectionString;
    }

    public void Dispose()
    {
        Connection.Close();
        Connection.Dispose();
    }
}