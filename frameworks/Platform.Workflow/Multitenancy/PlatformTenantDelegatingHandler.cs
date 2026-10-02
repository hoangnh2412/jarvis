using Microsoft.Extensions.Configuration;
using Platform.DDD.Domain.DataStorages;
using Platform.DDD.Domain.Services;

namespace Platform.Workflow.Multitenancy;

/// <summary>
/// DelegatingHandler tự động gắn header Tenant vào các HTTP request gửi tới Elsa Server (trong Standalone mode hoặc Studio client).
/// </summary>
public sealed class PlatformTenantDelegatingHandler(
    ICurrentTenantAccessor currentTenantAccessor,
    ITenantIdResolverFactory tenantIdResolverFactory,
    IConfiguration configuration) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var tenantId = currentTenantAccessor.TenantId
                       ?? await tenantIdResolverFactory.GetTenantIdAsync(cancellationToken).ConfigureAwait(false);
        if (tenantId.HasValue)
        {
            var headerKey = configuration.GetValue<string>("TenantHeaderKey") ?? "X-Tenant-Id";
            if (!request.Headers.Contains(headerKey))
            {
                request.Headers.Add(headerKey, tenantId.Value.ToString("D"));
            }
        }

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
