using System.Data.Common;
using Jarvis.Domain.DataStorages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Jarvis.EntityFramework.DataStorages;

/// <summary>
/// Applies the tenant connection string when a database connection opens via <see cref="ITenantConnectionStringResolverFactory"/>.
/// </summary>
public sealed class TenantDbConnectionInterceptor(
    ITenantConnectionStringResolverFactory connectionStringResolverFactory)
    : DbConnectionInterceptor
{
    private readonly ITenantConnectionStringResolverFactory _connectionStringResolverFactory = connectionStringResolverFactory;

    public override InterceptionResult ConnectionOpening(
        DbConnection connection,
        ConnectionEventData eventData,
        InterceptionResult result)
    {
        ApplyConnectionStringAsync(connection, eventData.Context, CancellationToken.None)
            .GetAwaiter()
            .GetResult();
        return base.ConnectionOpening(connection, eventData, result);
    }

    public override async ValueTask<InterceptionResult> ConnectionOpeningAsync(
        DbConnection connection,
        ConnectionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default)
    {
        await ApplyConnectionStringAsync(connection, eventData.Context, cancellationToken).ConfigureAwait(false);
        return await base.ConnectionOpeningAsync(connection, eventData, result, cancellationToken).ConfigureAwait(false);
    }

    private async Task ApplyConnectionStringAsync(
        DbConnection connection,
        DbContext? context,
        CancellationToken cancellationToken)
    {
        if (context == null)
            return;

        var connectionString = await _connectionStringResolverFactory
            .GetConnectionStringAsync(context.GetType(), cancellationToken)
            .ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(connectionString))
            connection.ConnectionString = connectionString;
    }
}
