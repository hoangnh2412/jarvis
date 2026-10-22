using Platform.DDD.Domain.DataStorages;
using Platform.DDD.Domain.Services;
using Platform.MultiOrg.DataStorages;
using Platform.MultiOrg.Scope;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Platform.MultiOrg;

public static class HostApplicationBuilderExtension
{
    /// <summary>Org: domain → <c>X-Org-Id</c> → query <c>orgId</c>.</summary>
    public static IHostApplicationBuilder AddOrgIdResolvers(this IHostApplicationBuilder builder)
    {
        builder.Services.TryAddKeyedScoped<IOrgIdResolver, OrgDomainIdResolver>(nameof(OrgDomainIdResolver));
        builder.Services.TryAddKeyedScoped<IOrgIdResolver, HeaderOrgIdResolver>(nameof(HeaderOrgIdResolver));
        builder.Services.TryAddKeyedScoped<IOrgIdResolver, QueryOrgIdResolver>(nameof(QueryOrgIdResolver));
        builder.Services.TryAddScoped<HeaderOrgIdResolver>();
        builder.Services.TryAddScoped<QueryOrgIdResolver>();
        builder.Services.TryAddScoped<IOrgIdResolverFactory, OrgIdResolverFactory>();
        builder.Services.TryAddScoped<IOrgTenantScopeValidator, NullOrgTenantScopeValidator>();
        return builder;
    }

    /// <summary>Org-first scope: tenant chỉ sau khi có org (<see cref="OrgFirstRequestScopeResolver"/>).</summary>
    public static IHostApplicationBuilder AddOrgFirstScopeResolution(this IHostApplicationBuilder builder)
    {
        builder.Services.TryAddScoped<IRequestScopeResolver, OrgFirstRequestScopeResolver>();
        builder.Services.TryAddScoped(sp =>
            new Lazy<IRequestScopeResolver>(() => sp.GetRequiredService<IRequestScopeResolver>()));
        return builder;
    }

    /// <summary>
    /// <see cref="ICurrentOrg{TOrg}"/>, <see cref="ICurrentOrgAccessor"/> và org id resolvers.
    /// Host bắt buộc đăng ký <see cref="ICurrentOrgStore{TOrg}"/>.
    /// </summary>
    public static IHostApplicationBuilder AddCurrentOrg<TOrg>(this IHostApplicationBuilder builder)
        where TOrg : class, ICurrentOrgIdentity
    {
        builder.AddOrgIdResolvers();
        builder.AddOrgFirstScopeResolution();
        builder.Services.TryAddSingleton<ICurrentOrgAccessor, CurrentOrgAccessor>();
        builder.Services.TryAddScoped<ICurrentOrg<TOrg>, CurrentOrg<TOrg>>();
        return builder;
    }
}
