using Jarvis.DDD.Domain.DataStorages;
using Jarvis.DDD.Domain.Repositories;
using Jarvis.ORM.EntityFramework.DataStorages;
using Jarvis.ORM.EntityFramework.Repositories;
using Jarvis.Caching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Jarvis.ORM.EntityFramework;

public static class HostApplicationBuilderExtension
{
    /// <summary>
    /// Register base repositories for Entity Framework (no tenant-id resolvers / interceptor).
    /// For dedicated tenant databases use <c>Jarvis.Multitenancy.EntityFramework</c>
    /// (<c>AddMultitenancyEntityFramework</c> + <c>AddCoreDbContext&lt;TDb, TResolver&gt;</c>).
    /// </summary>
    public static IHostApplicationBuilder AddEntityFramework(this IHostApplicationBuilder builder)
    {
        builder.AddRepositories();
        return builder;
    }

    private static IHostApplicationBuilder AddRepositories(this IHostApplicationBuilder builder)
    {
        RegisterCachedConnectionStringResolver(
            builder.Services,
            nameof(ConfigConnectionStringResolver),
            sp => new ConfigConnectionStringResolver(sp.GetRequiredService<IConfiguration>()));

        builder.Services.TryAddScoped(typeof(IQueryRepository<>), typeof(BaseQueryRepository<>));
        builder.Services.TryAddScoped(typeof(ICommandRepository<>), typeof(BaseCommandRepository<>));
        builder.Services.TryAddScoped(typeof(IRepository<>), typeof(BaseRepository<>));

        return builder;
    }

    /// <summary>
    /// Keyed <see cref="ITenantConnectionStringResolver"/> wrapped with <see cref="CachingTenantConnectionStringResolver"/>.
    /// Requires <c>AddJarvisCaching</c> before <c>AddEntityFramework</c>. Cache tiers: <c>Cache:Items</c> (memory / distributed via config).
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
                    "ICacheService is not registered. Call AddJarvisCaching() before AddEntityFramework().");

            return new CachingTenantConnectionStringResolver(
                createInner(sp),
                cacheService,
                cacheItemName,
                parameterName);
        });
    }

    /// <summary>
    /// Registers <see cref="IDbContextFactory{TContext}"/> with a fixed connection string from <paramref name="configure"/>
    /// (shared database or master database). Does not add a tenant connection interceptor — the connection string
    /// is taken only from <paramref name="configure"/>, not rewritten per tenant when the connection opens.
    /// Registers keyed <see cref="ConfigConnectionStringResolver"/> for <typeparamref name="TDbContext"/> and
    /// <see cref="ITenantConnectionStringResolverFactory"/> (used when other APIs resolve connection strings by DbContext name).
    /// Global query filters: <c>BaseUnitOfWork</c> sets tenant id on the context via <see cref="ITenantIdResolverFactory"/>.
    /// For per-tenant dedicated databases, use <c>Jarvis.Multitenancy.EntityFramework</c>
    /// <c>AddCoreDbContext&lt;TDbContext, TConnectionStringResolver&gt;</c>.
    /// </summary>
    public static IServiceCollection AddCoreDbContext<TDbContext>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder>? configure = null)
        where TDbContext : BaseStorageContext
    {
        RegisterCachedConnectionStringResolver(
            services,
            typeof(TDbContext).Name,
            sp => new ConfigConnectionStringResolver(sp.GetRequiredService<IConfiguration>()));
        services.TryAddScoped<ITenantConnectionStringResolverFactory, TenantConnectionStringResolverFactory>();

        services.AddDbContextFactory<TDbContext>((sp, options) =>
        {
            configure?.Invoke(options);
        });

        return services;
    }
}
