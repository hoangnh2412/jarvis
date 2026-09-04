using System.Data;
using System.Data.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Jarvis.ORM.Dapper;

/// <summary>
/// Default <see cref="ISqlConnectionFactory"/>: resolves connection strings from configuration
/// and opens connections via <see cref="OrmDapperOptions.CreateConnection"/>.
/// </summary>
public sealed class ConfigSqlConnectionFactory(
    IConfiguration configuration,
    IOptions<OrmDapperOptions> options) : ISqlConnectionFactory
{
    private readonly IConfiguration _configuration = configuration;
    private readonly OrmDapperOptions _options = options.Value;

    /// <inheritdoc />
    public Task<IDbConnection> OpenAsync(CancellationToken cancellationToken = default)
        => OpenAsync(_options.ConnectionStringName, cancellationToken);

    /// <inheritdoc />
    public async Task<IDbConnection> OpenAsync(string connectionStringName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionStringName);

        var create = _options.CreateConnection
            ?? throw new InvalidOperationException(
                "OrmDapperOptions.CreateConnection is not configured. " +
                "Call AddOrmDapper(o => o.CreateConnection = cs => new NpgsqlConnection(cs)) (or your ADO.NET provider).");

        var connectionString = _configuration.GetConnectionString(connectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{connectionStringName}' was not found under ConnectionStrings.");

        var connection = create(connectionString)
            ?? throw new InvalidOperationException("OrmDapperOptions.CreateConnection returned null.");

        if (connection.State == ConnectionState.Open)
            return connection;

        if (connection is DbConnection dbConnection)
            await dbConnection.OpenAsync(cancellationToken).ConfigureAwait(false);
        else
            connection.Open();

        return connection;
    }
}
