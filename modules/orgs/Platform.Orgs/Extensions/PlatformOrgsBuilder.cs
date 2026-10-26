using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Platform.DDD.Domain.Services;
using Platform.Orgs.Abstractions.Events;
using Platform.Orgs.Hooks;
using Platform.Orgs.Mapping;
using Platform.Orgs.Services;

namespace Platform.Orgs.Extensions;

/// <summary>Builder fluent sau <c>AddPlatformOrgs</c>.</summary>
public sealed class PlatformOrgsBuilder<TOrg, TTenant>
    where TOrg : class, IOrgCatalogEntity
    where TTenant : class, ICurrentTenantIdentity
{
    internal PlatformOrgsBuilder(IHostApplicationBuilder hostBuilder)
    {
        HostBuilder = hostBuilder;
    }

    public IHostApplicationBuilder HostBuilder { get; }

    /// <summary>Thay <see cref="DefaultOrgCatalogHooks{TOrg}"/> bằng hooks của host.</summary>
    public PlatformOrgsBuilder<TOrg, TTenant> UseHooks<THooks>()
        where THooks : class, IOrgCatalogHooks<TOrg>
    {
        HostBuilder.Services.AddScoped<IOrgCatalogHooks<TOrg>, THooks>();
        return this;
    }

    /// <summary>Đăng ký <see cref="IOrgCatalogReader{TReadModel}"/> + mapper host.</summary>
    public PlatformOrgsBuilder<TOrg, TTenant> UseReadModel<TReadModel, TMapper>()
        where TMapper : class, IOrgCatalogReadMapper<TOrg, TReadModel>
    {
        HostBuilder.Services.TryAddScoped<IOrgCatalogReadMapper<TOrg, TReadModel>, TMapper>();
        HostBuilder.Services.TryAddScoped<IOrgCatalogReader<TReadModel>, OrgCatalogReader<TOrg, TTenant, TReadModel>>();
        return this;
    }

    /// <summary>
    /// <see cref="ICurrentOrgStore{OrgCatalogOrgProfile}"/> đọc qua <see cref="IOrgCatalog"/>.
    /// Host gọi <c>AddCurrentOrg&lt;OrgCatalogOrgProfile&gt;()</c> (Platform.MultiOrg) trước/sau compose.
    /// </summary>
    public PlatformOrgsBuilder<TOrg, TTenant> UseAsCurrentOrgStore()
    {
        HostBuilder.Services.TryAddScoped<ICurrentOrgStore<OrgCatalogOrgProfile>, OrgCatalogCurrentOrgStore<TTenant>>();
        return this;
    }

    /// <summary>Thay publisher no-op. Gọi sau <c>AddPlatformOrgs</c>.</summary>
    public PlatformOrgsBuilder<TOrg, TTenant> UseOrgCatalogEventPublisher<TPublisher>()
        where TPublisher : class, IOrgCatalogEventPublisher
    {
        HostBuilder.Services.AddScoped<IOrgCatalogEventPublisher, TPublisher>();
        return this;
    }
}
