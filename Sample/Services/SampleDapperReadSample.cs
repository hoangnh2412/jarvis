using System.Data.Common;
using Dapper;
using Jarvis.ORM.Dapper;

namespace Sample.Services;

/// <summary>
/// Thin Sample for <c>Jarvis.ORM.Dapper</c>: one scalar read against the master connection.
/// </summary>
public sealed class SampleDapperReadSample(ISqlConnectionFactory connections)
{
    /// <summary>
    /// Executes <c>SELECT 1</c> via Dapper. Returns <c>1</c> when the connection works.
    /// </summary>
    public async Task<int> PingAsync(CancellationToken cancellationToken = default)
    {
        var connection = await connections.OpenAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await connection.ExecuteScalarAsync<int>(
                    new CommandDefinition("SELECT 1", cancellationToken: cancellationToken))
                .ConfigureAwait(false);
        }
        finally
        {
            if (connection is DbConnection db)
                await db.DisposeAsync().ConfigureAwait(false);
            else
                connection.Dispose();
        }
    }
}
