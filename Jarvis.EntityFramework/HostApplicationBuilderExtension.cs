using Jarvis.Domain.DataStorages;
using Jarvis.Domain.Repositories;
using Jarvis.EntityFramework.DataStorages;
using Jarvis.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;
using Jarvis.Caching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Jarvis.EntityFramework;

public static class HostApplicationBuilderExtension
{
    /// <summary>
    /// Register base repositories for Entify Framework
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IHostApplicationBuilder AddEntityFramework(this IHostApplicationBuilder builder)
    {
        builder.AddRepositories();
        builder.AddMultitenancy();

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
    /// Registers keyed <see cref="ITenantIdResolver"/> implementations (header, query, user, host). Does not register any <see cref="ITenantConnectionStringResolver"/> — the host app must register how connection strings are resolved (config, header, custom type, etc.).
    /// </summary>
    private static IHostApplicationBuilder AddMultitenancy(this IHostApplicationBuilder builder)
    {
        builder.Services.TryAddKeyedScoped<ITenantIdResolver, HeaderTenantIdResolver>(nameof(HeaderTenantIdResolver));
        builder.Services.TryAddKeyedScoped<ITenantIdResolver, QueryTenantIdResolver>(nameof(QueryTenantIdResolver));
        builder.Services.TryAddKeyedScoped<ITenantIdResolver, UserTenantIdResolver>(nameof(UserTenantIdResolver));
        builder.Services.TryAddKeyedScoped<ITenantIdResolver, HostTenantIdResolver>(nameof(HostTenantIdResolver));
        builder.Services.TryAddSingleton<ICurrentTenantAccessor, CurrentTenantAccessor>();
        builder.Services.TryAddScoped<ITenantIdResolverFactory, TenantIdResolverFactory>();
        return builder;
    }

    /// <summary>
    /// Registers <see cref="IDbContextFactory{TContext}"/> for dedicated per-tenant databases.
    /// Adds <see cref="TenantDbConnectionInterceptor"/>, which resolves the connection string when the connection opens
    /// (<see cref="ITenantIdResolverFactory"/> + keyed <see cref="ITenantConnectionStringResolver"/> for
    /// <typeparamref name="TDbContext"/> via <see cref="ITenantConnectionStringResolverFactory"/>).
    /// Registers <typeparamref name="TConnectionStringResolver"/> as the inner fallback (DB, config, API, …); exposed keyed resolver is always cached.
    /// </summary>
    /// <typeparam name="TDbContext">Concrete <see cref="DbContext"/> deriving from <see cref="BaseStorageContext{TDbContext}"/>.</typeparam>
    /// <typeparam name="TConnectionStringResolver">Inner <see cref="ITenantConnectionStringResolver"/> invoked on cache miss.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">
    /// Optional provider setup with a placeholder connection string (overwritten by the interceptor), e.g.
    /// <c>options =&gt; options.UseNpgsql("Host=localhost;Database=placeholder")</c>.
    /// </param>
    public static IServiceCollection AddCoreDbContext<TDbContext, TConnectionStringResolver>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder>? configure = null)
        where TDbContext : BaseStorageContext<TDbContext>
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
    /// Registers <see cref="IDbContextFactory{TContext}"/> with a fixed connection string from <paramref name="configure"/>
    /// (shared database or master database). Does not add <see cref="TenantDbConnectionInterceptor"/> — the connection string
    /// is taken only from <paramref name="configure"/>, not rewritten per tenant when the connection opens.
    /// Registers keyed <see cref="ConfigConnectionStringResolver"/> for <typeparamref name="TDbContext"/> and
    /// <see cref="ITenantConnectionStringResolverFactory"/> (used when other APIs resolve connection strings by DbContext name).
    /// Global query filters: <c>BaseUnitOfWork</c> sets tenant id on the context via <see cref="ITenantIdResolverFactory"/>.
    /// For per-tenant dedicated databases, use <see cref="AddCoreDbContext{TDbContext, TConnectionStringResolver}"/>.
    /// </summary>
    /// <typeparam name="TDbContext">Concrete <see cref="DbContext"/> deriving from <see cref="BaseStorageContext{TDbContext}"/>.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">
    /// Provider setup with the real connection string, e.g.
    /// <c>options =&gt; options.UseNpgsql(configuration.GetConnectionString("MasterDbContext"))</c>.
    /// </param>
    public static IServiceCollection AddCoreDbContext<TDbContext>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder>? configure = null)
        where TDbContext : BaseStorageContext<TDbContext>
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
