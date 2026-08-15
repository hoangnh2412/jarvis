using Jarvis.DDD.Domain.DataStorages;
using Jarvis.DDD.Domain.Services;
using Jarvis.Multitenancy.DataStorages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Jarvis.Multitenancy;

public static class HostApplicationBuilderExtension
{
    /// <summary>
    /// Đăng ký keyed <see cref="ITenantIdResolver"/> (header → user → query → host) và
    /// <see cref="ITenantIdResolverFactory"/>. Được gọi từ <see cref="AddCurrentTenant{TTenant}"/>.
    /// </summary>
    public static IHostApplicationBuilder AddTenantIdResolvers(this IHostApplicationBuilder builder)
    {
        builder.Services.TryAddKeyedScoped<ITenantIdResolver, HeaderTenantIdResolver>(nameof(HeaderTenantIdResolver));
        builder.Services.TryAddKeyedScoped<ITenantIdResolver, QueryTenantIdResolver>(nameof(QueryTenantIdResolver));
        builder.Services.TryAddKeyedScoped<ITenantIdResolver, UserTenantIdResolver>(nameof(UserTenantIdResolver));
        builder.Services.TryAddKeyedScoped<ITenantIdResolver, HostTenantIdResolver>(nameof(HostTenantIdResolver));
        builder.Services.TryAddScoped<ITenantIdResolverFactory, TenantIdResolverFactory>();
        return builder;
    }

    /// <summary>
    /// Đăng ký <see cref="ICurrentTenant{TTenant}"/>, <see cref="ICurrentTenantAccessor"/> và tenant id resolvers.
    /// Host bắt buộc đăng ký <see cref="ICurrentTenantStore{TTenant}"/>.
    /// </summary>
    public static IHostApplicationBuilder AddCurrentTenant<TTenant>(this IHostApplicationBuilder builder)
        where TTenant : class, ICurrentTenantIdentity
    {
        builder.AddTenantIdResolvers();
        builder.Services.TryAddSingleton<ICurrentTenantAccessor, CurrentTenantAccessor>();
        builder.Services.TryAddScoped<ICurrentTenant<TTenant>, CurrentTenant<TTenant>>();
        return builder;
    }
}
