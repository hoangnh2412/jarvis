using System.Data;

namespace Jarvis.ORM.Dapper;

/// <summary>
/// Configuration for <see cref="HostApplicationBuilderExtension.AddOrmDapper"/>.
/// Host supplies <see cref="CreateConnection"/> for the ADO.NET provider (Npgsql, SqlClient, …).
/// </summary>
public sealed class OrmDapperOptions
{
    /// <summary>
    /// Default <c>ConnectionStrings</c> name used by <see cref="ISqlConnectionFactory.OpenAsync(CancellationToken)"/>.
    /// </summary>
    public string ConnectionStringName { get; set; } = "Default";

    /// <summary>
    /// Creates a closed <see cref="IDbConnection"/> from a connection string.
    /// Required — e.g. <c>cs =&gt; new NpgsqlConnection(cs)</c>.
    /// </summary>
    public Func<string, IDbConnection>? CreateConnection { get; set; }
}
