using Microsoft.Extensions.DependencyInjection;
using Platform.Tenants.Api.Controllers;
using Platform.Tenants.Extensions;

namespace Platform.Tenants.Api.Extensions;

/// <summary>Đăng ký HTTP API vận hành tenant tại <c>/platform/tenants</c>.</summary>
public static class PlatformTenantsApiExtensions
{
    public static PlatformTenantsBuilder<TTenant> UseHttpApi<TTenant>(this PlatformTenantsBuilder<TTenant> builder)
        where TTenant : class, ITenantCatalogEntity
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.HostBuilder.Services
            .AddControllers()
            .AddApplicationPart(typeof(TenantController).Assembly);
        return builder;
    }
}
