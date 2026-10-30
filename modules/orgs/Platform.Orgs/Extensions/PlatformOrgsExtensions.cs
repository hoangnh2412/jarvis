using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Platform.DDD.Domain.Services;
using Platform.Orgs.Abstractions;
using Platform.Orgs.Abstractions.Events;
using Platform.Orgs.Events;
using Platform.Orgs.Hooks;
using Platform.Orgs.Services;

namespace Platform.Orgs.Extensions;

/// <summary>Đăng ký catalog Org. Store EF đăng ký bằng <c>UseEntityFramework</c>.</summary>
public static class PlatformOrgsExtensions
{
    public static PlatformOrgsBuilder<TOrg, TTenant> AddPlatformOrgs<TOrg, TTenant>(this IHostApplicationBuilder builder)
        where TOrg : class, IOrgManagementEntity
        where TTenant : class, ICurrentTenantIdentity
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.TryAddScoped<IOrgCatalogHooks<TOrg>, DefaultOrgCatalogHooks<TOrg>>();
        builder.Services.TryAddScoped<IOrgTenantVisibility, SelfOrgTenantVisibility>();
        builder.Services.TryAddScoped<IOrgManager, OrgManager<TOrg, TTenant>>();
        builder.Services.TryAddScoped<IOrgCatalog, OrgCatalog<TOrg>>();
        builder.Services.AddScoped<IOrgTenantScopeValidator, OrgCatalogTenantScopeValidator>();
        builder.Services.TryAddSingleton<IOrgCatalogEventPublisher, NullOrgCatalogEventPublisher>();

        return new PlatformOrgsBuilder<TOrg, TTenant>(builder);
    }
}
