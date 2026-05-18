using Jarvis.Domain.DataStorages;
using Microsoft.EntityFrameworkCore;
using Sample.Persistence;

namespace Sample.Multitenancy;

public sealed class TenantEfTestService(IServiceScopeFactory scopeFactory)
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    /// <summary>
    /// Simulates a background job: new scope, set tenant id, resolve connection string, open tenant DbContext.
    /// </summary>
    public async Task<TenantEfTestResult> RunBackgroundJobAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var jobContext = scope.ServiceProvider.GetRequiredService<JobTenantContext>();
        jobContext.TenantId = tenantId;

        var connectionStringFactory = scope.ServiceProvider.GetRequiredService<ITenantConnectionStringResolverFactory>();
        var connectionString = await connectionStringFactory
            .GetConnectionStringAsync(typeof(TenantDbContext), cancellationToken)
            .ConfigureAwait(false);

        var uow = scope.ServiceProvider.GetRequiredService<ISampleUnitOfWork>();
        var dbContext = (TenantDbContext)await uow.GetDbContextAsync(cancellationToken).ConfigureAwait(false);

        var database = await GetDatabaseNameAsync(dbContext, cancellationToken).ConfigureAwait(false);
        var studentCount = await dbContext.Students.CountAsync(cancellationToken).ConfigureAwait(false);

        return new TenantEfTestResult(
            Scenario: "background-job",
            ResolvedTenantId: tenantId,
            ConnectionStringPreview: ConnectionStringHelper.Mask(connectionString),
            Database: database,
            StudentCount: studentCount,
            Hint: connectionString == null
                ? "No connection string resolved. Ensure Master.Tenant has a row for this tenant id."
                : null);
    }

    public static async Task<TenantEfTestResult> RunHttpRequestAsync(
        ISampleUnitOfWork unitOfWork,
        ITenantIdResolverFactory tenantIdResolverFactory,
        ITenantConnectionStringResolverFactory connectionStringFactory,
        CancellationToken cancellationToken = default)
    {
        var tenantId = await tenantIdResolverFactory.GetTenantIdAsync(cancellationToken).ConfigureAwait(false);
        var connectionString = await connectionStringFactory
            .GetConnectionStringAsync(typeof(TenantDbContext), cancellationToken)
            .ConfigureAwait(false);

        var dbContext = (TenantDbContext)await unitOfWork.GetDbContextAsync(cancellationToken).ConfigureAwait(false);

        var database = await GetDatabaseNameAsync(dbContext, cancellationToken).ConfigureAwait(false);
        var studentCount = await dbContext.Students.CountAsync(cancellationToken).ConfigureAwait(false);

        return new TenantEfTestResult(
            Scenario: "http-header",
            ResolvedTenantId: tenantId,
            ConnectionStringPreview: ConnectionStringHelper.Mask(connectionString),
            Database: database,
            StudentCount: studentCount,
            Hint: tenantId == null
                ? "Send header X-Tenant-Id with a tenant Guid registered in Master.Tenant, or use default ConnectionStrings:TenantDbContext."
                : null);
    }

    private static async Task<string?> GetDatabaseNameAsync(TenantDbContext dbContext, CancellationToken cancellationToken)
    {
        var connection = dbContext.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await dbContext.Database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        return connection.Database;
    }
}

public static class ConnectionStringHelper
{
    public static string? Mask(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return null;

        var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        for (var i = 0; i < parts.Length; i++)
        {
            if (parts[i].StartsWith("Password=", StringComparison.OrdinalIgnoreCase)
                || parts[i].StartsWith("Pwd=", StringComparison.OrdinalIgnoreCase))
            {
                var eq = parts[i].IndexOf('=');
                parts[i] = eq >= 0 ? parts[i][..(eq + 1)] + "***" : "***";
            }
        }

        return string.Join(';', parts);
    }
}
