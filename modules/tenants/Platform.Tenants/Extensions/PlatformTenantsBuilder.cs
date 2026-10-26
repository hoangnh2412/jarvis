using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Platform.DDD.Domain.Services;
using Platform.Tenants.Services;

namespace Platform.Tenants.Extensions;

/// <summary>Builder fluent sau <c>AddPlatformTenants</c>.</summary>
public sealed class PlatformTenantsBuilder<TTenant>
    where TTenant : class, ITenantCatalogEntity
{
    internal PlatformTenantsBuilder(IHostApplicationBuilder hostBuilder)
    {
        HostBuilder = hostBuilder;
    }

    public IHostApplicationBuilder HostBuilder { get; }

    /// <summary>
    /// <see cref="ICurrentTenantStore{T}"/> đọc catalog. Host vẫn gọi <c>AddCurrentTenant</c> riêng.
    /// </summary>
    public PlatformTenantsBuilder<TTenant> UseAsCurrentTenantStore()
    {
        HostBuilder.Services.TryAddScoped<ICurrentTenantStore<TenantCatalogProfile>, TenantCatalogCurrentTenantStore>();
        return this;
    }
}
