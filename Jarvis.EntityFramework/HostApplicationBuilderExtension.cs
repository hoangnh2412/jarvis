using Jarvis.Domain.DataStorages;
using Jarvis.Domain.Repositories;
using Jarvis.EntityFramework.DataStorages;
using Jarvis.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

    public static IHostApplicationBuilder AddRepositories(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped(typeof(IQueryRepository<>), typeof(BaseQueryRepository<>));
        builder.Services.AddScoped(typeof(ICommandRepository<>), typeof(BaseCommandRepository<>));
        builder.Services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));

        return builder;
    }

    /// <summary>
    /// Registers keyed <see cref="ITenantIdResolver"/> implementations (header, query, user, host). Does not register any <see cref="ITenantConnectionStringResolver"/> — the host app must register how connection strings are resolved (config, header, custom type, etc.).
    /// </summary>
    public static IHostApplicationBuilder AddMultitenancy(this IHostApplicationBuilder builder)
    {
        builder.Services.AddKeyedScoped<ITenantIdResolver, HeaderTenantIdResolver>(nameof(HeaderTenantIdResolver));
        builder.Services.AddKeyedScoped<ITenantIdResolver, QueryTenantIdResolver>(nameof(QueryTenantIdResolver));
        builder.Services.AddKeyedScoped<ITenantIdResolver, UserTenantIdResolver>(nameof(UserTenantIdResolver));
        builder.Services.AddKeyedScoped<ITenantIdResolver, HostTenantIdResolver>(nameof(HostTenantIdResolver));
        return builder;
    }

    /// <summary>
    /// Calls <see cref="AddMultitenancy"/> and <c>AddMemoryCache</c>. Use when your connection-string resolver (or other components) needs <see cref="Microsoft.Extensions.Caching.Memory.IMemoryCache"/>.
    /// </summary>
    public static IHostApplicationBuilder AddMultitenancyWithMemoryCache(this IHostApplicationBuilder builder)
    {
        builder.AddMultitenancy();
        builder.Services.AddMemoryCache();
        return builder;
    }

    /// <summary>
    /// Registers <see cref="ConfigConnectionStringResolver"/> as keyed <see cref="ITenantConnectionStringResolver"/> using the same key as the type name <c>ConfigConnectionStringResolver</c>. Optional convenience for apps that read connection strings from configuration.
    /// </summary>
    public static IServiceCollection AddKeyedConfigConnectionStringResolver(this IServiceCollection services)
    {
        services.AddKeyedScoped<ITenantConnectionStringResolver, ConfigConnectionStringResolver>(nameof(ConfigConnectionStringResolver));
        return services;
    }

    /// <summary>
    /// Registers a keyed <see cref="ITenantConnectionStringResolver"/> that caches results from <see cref="ConfigConnectionStringResolver"/> (same key as <see cref="AddKeyedConfigConnectionStringResolver"/>). Calls <c>AddMemoryCache</c> if not already registered. Do not combine with <see cref="AddKeyedConfigConnectionStringResolver"/> for the same key.
    /// </summary>
    public static IServiceCollection AddKeyedCachingConfigConnectionStringResolver(
        this IServiceCollection services,
        TimeSpan? slidingExpiration = null)
    {
        services.AddMemoryCache();
        services.AddKeyedSingleton<ITenantConnectionStringResolver>(nameof(ConfigConnectionStringResolver), (sp, _) =>
            new CachingTenantConnectionStringResolver(
                new ConfigConnectionStringResolver(sp.GetRequiredService<IConfiguration>()),
                sp.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>(),
                slidingExpiration));
        return services;
    }

    /// <summary>
    /// Registers <see cref="IDbContextFactory{TContext}"/> via <c>AddDbContextFactory</c> and scoped <see cref="IStorageContextTenantInitializer"/> for <see cref="IUnitOfWork"/>.
    /// </summary>
    /// <typeparam name="TDbContext">Concrete <see cref="DbContext"/> deriving from <see cref="BaseStorageContext{TDbContext}"/>.</typeparam>
    /// <typeparam name="TTenantIdResolver">
    /// Implementation type used to resolve the current tenant key (must match a keyed <see cref="ITenantIdResolver"/> registration).
    /// Use <see cref="HeaderTenantIdResolver"/> (header), <see cref="QueryTenantIdResolver"/> (query string), <see cref="UserTenantIdResolver"/> (claims), <see cref="HostTenantIdResolver"/> (request host), or your own type registered with the same name via <c>AddKeyedScoped&lt;ITenantIdResolver, YourResolver&gt;(nameof(YourResolver))</c>.
    /// </typeparam>
    /// <typeparam name="TConnectionStringResolver">
    /// Implementation type for connection string lookup (must match a keyed <see cref="ITenantConnectionStringResolver"/> registration that you add in the host, e.g. <see cref="AddKeyedConfigConnectionStringResolver"/> or your own resolver).
    /// </typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">
    /// Configure <see cref="DbContextOptionsBuilder"/> using the resolved tenant and connection resolvers (e.g. call <c>UseNpgsql(connectionString)</c>).
    /// </param>
    public static IServiceCollection AddCoreDbContext<TDbContext, TTenantIdResolver, TConnectionStringResolver>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder, ITenantIdResolver, ITenantConnectionStringResolver> configure)
        where TDbContext : BaseStorageContext<TDbContext>
        where TTenantIdResolver : ITenantIdResolver
        where TConnectionStringResolver : ITenantConnectionStringResolver
    {
        var tenantKey = typeof(TTenantIdResolver).Name;
        var connectionKey = typeof(TConnectionStringResolver).Name;

        services.AddDbContextFactory<TDbContext>((sp, options) =>
        {
            using var scope = sp.CreateScope();
            var scoped = scope.ServiceProvider;
            configure(
                options,
                scoped.GetRequiredKeyedService<ITenantIdResolver>(tenantKey),
                scoped.GetRequiredKeyedService<ITenantConnectionStringResolver>(connectionKey));
        });

        services.AddScoped<IStorageContextTenantInitializer>(sp =>
            new StorageContextTenantInitializer(sp.GetRequiredKeyedService<ITenantIdResolver>(tenantKey)));

        return services;
    }
}
