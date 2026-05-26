using Jarvis.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Sample.Entities;
using Sample.Persistence;

namespace UnitTest.Infrastructure;

internal static class MultitenancyEfTestDataSeeder
{
    public static async Task EnsureTenantRegisteredAsync(
        IServiceProvider serviceProvider,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var masterUow = scope.ServiceProvider.GetRequiredService<IMasterUnitOfWork>();
        var repoTenant = await masterUow.GetRepositoryAsync<IRepository<Tenant>>(cancellationToken);

        var tenant = await repoTenant
            .GetByIdAsync(x => x.Id == tenantId, cancellationToken)
            .ConfigureAwait(false);

        if (tenant != null)
            return;

        await repoTenant.InsertAsync(new Tenant
        {
            Id = tenantId,
            ConnectionString = MultitenancyEfTestDatabaseNames.Tenant(tenantId),
        }, cancellationToken).ConfigureAwait(false);

        await masterUow.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
