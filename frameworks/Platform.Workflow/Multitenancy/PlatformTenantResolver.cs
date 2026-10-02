using Elsa.Common.Multitenancy;
using Microsoft.Extensions.DependencyInjection;
using Platform.DDD.Domain.DataStorages;
using Platform.DDD.Domain.Services;

namespace Platform.Workflow.Multitenancy;

/// <summary>
/// Tenant resolver adapter cho Elsa pipeline dựa trên tenant context của Platform.
/// Singleton (Elsa yêu cầu) — resolve scoped <see cref="ITenantIdResolverFactory"/> qua scope factory.
/// </summary>
public sealed class PlatformTenantResolver(
    ICurrentTenantAccessor currentTenantAccessor,
    IServiceScopeFactory scopeFactory) : TenantResolverBase
{
    protected override async Task<TenantResolverResult> ResolveAsync(TenantResolverContext context)
    {
        var tenantId = currentTenantAccessor.TenantId;
        if (!tenantId.HasValue)
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var tenantIdResolverFactory = scope.ServiceProvider.GetRequiredService<ITenantIdResolverFactory>();
            tenantId = await tenantIdResolverFactory.GetTenantIdAsync(context.CancellationToken).ConfigureAwait(false);
        }

        if (tenantId.HasValue)
        {
            return Resolved(tenantId.Value.ToString("D"));
        }

        return Unresolved();
    }
}
