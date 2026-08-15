using System.Data;

namespace Jarvis.ORM.Dapper;

/// <summary>
/// Opens ADO.NET connections for Dapper read/report queries (shared / named connection strings).
/// Tenant-dedicated resolution belongs in a Multitenancy.Dapper satellite (later), not this foundation.
/// </summary>
public interface ISqlConnectionFactory
{
    /// <summary>
    /// Opens a connection using <see cref="OrmDapperOptions.ConnectionStringName"/>.
    /// Caller owns dispose.
    /// </summary>
    Task<IDbConnection> OpenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens a connection for the given <c>ConnectionStrings</c> name.
    /// Caller owns dispose.
    /// </summary>
    Task<IDbConnection> OpenAsync(string connectionStringName, CancellationToken cancellationToken = default);
}
