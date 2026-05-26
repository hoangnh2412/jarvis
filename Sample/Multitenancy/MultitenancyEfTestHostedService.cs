using Jarvis.OpenTelemetry.HostedServices;

namespace Sample.Multitenancy;

/// <summary>
/// Periodically runs Master-only and Master+Tenant background EF multitenancy demos.
/// </summary>
public sealed class MultitenancyEfTestHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<MultitenancyEfTestHostedService> logger)
    : BaseWorker(scopeFactory, logger)
{
    private static readonly TimeSpan StartupDelay = TimeSpan.FromSeconds(5);
    private static readonly Guid TestTenantId = MultitenancyEfJobRunner.MasterTenantRegistryId;

    /// <summary>Every minute (replaces the previous 60s fixed delay loop).</summary>
    protected override string CronExpression => "* * * * *";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(StartupDelay, stoppingToken).ConfigureAwait(false);

        logger.LogInformation(
            "Multitenancy EF test hosted service started. Cron={CronExpression}, TenantId={TenantId}",
            CronExpression,
            TestTenantId);

        await base.ExecuteAsync(stoppingToken).ConfigureAwait(false);
    }

    protected override async Task ExecuteJobAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var runner = serviceProvider.GetRequiredService<MultitenancyEfJobRunner>();

        var masterResult = await runner
            .RunMasterWithoutTenantAsync(serviceProvider, cancellationToken)
            .ConfigureAwait(false);

        logger.LogInformation(
            "Job [master]: tenant {TenantId}, rows before={BeforeCount} after={AfterCount}",
            masterResult.TenantId,
            masterResult.BeforeCount,
            masterResult.AfterCount);

        var tenantResult = await runner
            .RunMasterTenantWithTenantIdAsync(serviceProvider, Guid.Parse("238c06f2-aa2a-406f-bb94-d1646211f741"), cancellationToken)
            .ConfigureAwait(false);

        logger.LogInformation(
            "Job [master-tenant]: tenant={TenantId}, students before={BeforeCount} after={AfterCount}, student={StudentName}",
            tenantResult.TenantId,
            tenantResult.BeforeCount,
            tenantResult.AfterCount,
            tenantResult.StudentName);
    }
}
