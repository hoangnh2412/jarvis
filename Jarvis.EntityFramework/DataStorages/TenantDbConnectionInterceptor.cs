using System.Data.Common;
using Jarvis.Domain.DataStorages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.EntityFramework.DataStorages;

/// <summary>
/// Applies the tenant connection string when a database connection opens via <see cref="ITenantConnectionStringResolverFactory"/>.
/// Registered as singleton; resolves scoped services per connection via <see cref="IServiceScopeFactory"/>.
/// </summary>
public sealed class TenantDbConnectionInterceptor(IServiceScopeFactory scopeFactory)
    : DbConnectionInterceptor
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

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

        await using var scope = _scopeFactory.CreateAsyncScope();
        var connectionStringResolverFactory = scope.ServiceProvider
            .GetRequiredService<ITenantConnectionStringResolverFactory>();

        var connectionString = await connectionStringResolverFactory
            .GetConnectionStringAsync(context.GetType(), cancellationToken)
            .ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(connectionString))
            connection.ConnectionString = connectionString;
    }
}
