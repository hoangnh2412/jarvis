using Jarvis.Caching;
using Jarvis.DDD.Domain.DataStorages;
using Jarvis.ORM.EntityFramework.DataStorages;
using Jarvis.Multitenancy.EntityFramework.DataStorages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Jarvis.Multitenancy.EntityFramework;

public static class HostApplicationBuilderExtension
{
    /// <summary>
    /// Opt-in Multitenancy + EF persistence: registers <see cref="TenantDbConnectionInterceptor"/> and
    /// <see cref="ITenantConnectionStringResolverFactory"/>. Call after <c>AddEntityFramework</c> / caching.
    /// Use <see cref="AddCoreDbContext{TDbContext, TConnectionStringResolver}"/> for dedicated tenant databases.
    /// </summary>
    public static IHostApplicationBuilder AddMultitenancyEntityFramework(this IHostApplicationBuilder builder)
    {
        builder.Services.TryAddScoped<ITenantConnectionStringResolverFactory, TenantConnectionStringResolverFactory>();
        builder.Services.TryAddSingleton<TenantDbConnectionInterceptor>();
        return builder;
    }

    /// <summary>
    /// Registers <see cref="IDbContextFactory{TContext}"/> for dedicated per-tenant databases.
    /// Adds <see cref="TenantDbConnectionInterceptor"/>, which resolves the connection string when the connection opens
    /// (<see cref="ITenantIdResolverFactory"/> + keyed <see cref="ITenantConnectionStringResolver"/> for
    /// <typeparamref name="TDbContext"/> via <see cref="ITenantConnectionStringResolverFactory"/>).
    /// Registers <typeparamref name="TConnectionStringResolver"/> as the inner fallback (DB, config, API, …); exposed keyed resolver is always cached.
    /// </summary>
    public static IServiceCollection AddCoreDbContext<TDbContext, TConnectionStringResolver>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder>? configure = null)
        where TDbContext : BaseStorageContext
        where TConnectionStringResolver : class, ITenantConnectionStringResolver
    {
        services.TryAddScoped<TConnectionStringResolver>();
        RegisterCachedConnectionStringResolver(
            services,
            typeof(TDbContext).Name,
            sp => sp.GetRequiredService<TConnectionStringResolver>());
        services.TryAddScoped<ITenantConnectionStringResolverFactory, TenantConnectionStringResolverFactory>();
        services.TryAddSingleton<TenantDbConnectionInterceptor>();

        services.AddDbContextFactory<TDbContext>((sp, options) =>
        {
            configure?.Invoke(options);
            options.AddInterceptors(sp.GetRequiredService<TenantDbConnectionInterceptor>());
        });
        return services;
    }

    /// <summary>
    /// Keyed <see cref="ITenantConnectionStringResolver"/> wrapped with <see cref="CachingTenantConnectionStringResolver"/>.
    /// Requires <c>AddJarvisCaching</c> before registration. Cache tiers: <c>Cache:Items</c>.
    /// </summary>
    private static void RegisterCachedConnectionStringResolver(
        IServiceCollection services,
        string serviceKey,
        Func<IServiceProvider, ITenantConnectionStringResolver> createInner,
        string cacheItemName = CachingTenantConnectionStringResolver.DefaultCacheItemName,
        string parameterName = CachingTenantConnectionStringResolver.DefaultParameterName)
    {
        services.RemoveAllKeyed<ITenantConnectionStringResolver>(serviceKey);
        services.AddKeyedScoped<ITenantConnectionStringResolver>(serviceKey, (sp, _) =>
        {
            var cacheService = sp.GetService<ICacheService>()
                ?? throw new InvalidOperationException(
                    "ICacheService is not registered. Call AddJarvisCaching() before AddMultitenancyEntityFramework() / AddCoreDbContext.");

            return new CachingTenantConnectionStringResolver(
                createInner(sp),
                cacheService,
                cacheItemName,
                parameterName);
        });
    }
}
