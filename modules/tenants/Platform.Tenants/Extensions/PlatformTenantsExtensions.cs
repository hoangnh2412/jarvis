using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Platform.Tenants.Services;

namespace Platform.Tenants.Extensions;

/// <summary>Đăng ký catalog tenant. Store EF đăng ký bằng <c>UseEntityFramework</c>.</summary>
public static class PlatformTenantsExtensions
{
    public static PlatformTenantsBuilder<TTenant> AddPlatformTenants<TTenant>(this IHostApplicationBuilder builder)
        where TTenant : class, ITenantCatalogEntity
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Services.TryAddScoped<ITenantManager, TenantManager<TTenant>>();
        return new PlatformTenantsBuilder<TTenant>(builder);
    }
}
