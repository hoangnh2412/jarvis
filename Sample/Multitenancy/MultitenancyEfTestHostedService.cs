namespace Sample.Multitenancy;

/// <summary>
/// Periodically runs Master-only and Master+Tenant background EF multitenancy demos.
/// </summary>
public sealed class MultitenancyEfTestHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<MultitenancyEfTestHostedService> logger)
    : BackgroundService
{
    private static readonly TimeSpan StartupDelay = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(60);
    private static readonly Guid TestTenantId = MultitenancyEfJobRunner.MasterTenantRegistryId;

    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<MultitenancyEfTestHostedService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(StartupDelay, stoppingToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Multitenancy EF test hosted service started. Interval={IntervalSeconds}s, TenantId={TenantId}",
            Interval.TotalSeconds,
            TestTenantId);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunOnceAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Multitenancy EF test job failed");
            }

            await Task.Delay(Interval, stoppingToken).ConfigureAwait(false);
        }
    }

    private async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var runner = scope.ServiceProvider.GetRequiredService<MultitenancyEfJobRunner>();

        var masterResult = await runner
            .RunMasterWithoutTenantAsync(scope.ServiceProvider, cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation(
            "Job [master]: tenant {TenantId}, rows before={BeforeCount} after={AfterCount}",
            masterResult.TenantId,
            masterResult.BeforeCount,
            masterResult.AfterCount);

        var tenantResult = await runner
            .RunMasterTenantWithTenantIdAsync(scope.ServiceProvider, Guid.Parse("238c06f2-aa2a-406f-bb94-d1646211f741"), cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation(
            "Job [master-tenant]: tenant={TenantId}, students before={BeforeCount} after={AfterCount}, student={StudentName}",
            tenantResult.TenantId,
            tenantResult.BeforeCount,
            tenantResult.AfterCount,
            tenantResult.StudentName);
    }
}
