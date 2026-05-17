using Jarvis.Domain.DataStorages;
using Jarvis.Domain.Repositories;
using Jarvis.EntityFramework.DataStorages;
using Jarvis.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;
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
        builder.Services.TryAddScoped(typeof(IQueryRepository<>), typeof(BaseQueryRepository<>));
        builder.Services.TryAddScoped(typeof(ICommandRepository<>), typeof(BaseCommandRepository<>));
        builder.Services.TryAddScoped(typeof(IRepository<>), typeof(BaseRepository<>));

        return builder;
    }

    /// <summary>
    /// Registers keyed <see cref="ITenantIdResolver"/> implementations (header, query, user, host). Does not register any <see cref="ITenantConnectionStringResolver"/> — the host app must register how connection strings are resolved (config, header, custom type, etc.).
    /// </summary>
    private static IHostApplicationBuilder AddMultitenancy(this IHostApplicationBuilder builder)
    {
        builder.Services.AddKeyedScoped<ITenantIdResolver, HeaderTenantIdResolver>(nameof(HeaderTenantIdResolver));
        builder.Services.AddKeyedScoped<ITenantIdResolver, QueryTenantIdResolver>(nameof(QueryTenantIdResolver));
        builder.Services.AddKeyedScoped<ITenantIdResolver, UserTenantIdResolver>(nameof(UserTenantIdResolver));
        builder.Services.AddKeyedScoped<ITenantIdResolver, HostTenantIdResolver>(nameof(HostTenantIdResolver));
        builder.Services.TryAddScoped<ITenantIdResolverFactory, TenantIdResolverFactory>();
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
        services.AddKeyedSingleton<ITenantConnectionStringResolver>(nameof(ConfigConnectionStringResolver), (sp, _) =>
            new CachingTenantConnectionStringResolver(
                new ConfigConnectionStringResolver(sp.GetRequiredService<IConfiguration>()),
                sp.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>(),
                slidingExpiration));
        return services;
    }

    /// <summary>
    /// Registers <see cref="IDbContextFactory{TContext}"/> with <see cref="TenantDbConnectionInterceptor"/>.
    /// Tenant id is resolved via <see cref="ITenantIdResolverFactory"/> (header → user → query → host) when the connection opens.
    /// </summary>
    /// <typeparam name="TDbContext">Concrete <see cref="DbContext"/> deriving from <see cref="BaseStorageContext{TDbContext}"/>.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">
    /// Optional provider setup with a placeholder connection string (overwritten by the interceptor), e.g.
    /// <c>options =&gt; options.UseNpgsql("Host=localhost;Database=placeholder")</c>.
    /// </param>
    public static IServiceCollection AddCoreDbContext<TDbContext>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder>? configure = null)
        where TDbContext : BaseStorageContext<TDbContext>
    {
        services.TryAddScoped<TenantDbConnectionInterceptor>();

        services.AddDbContextFactory<TDbContext>((sp, options) =>
        {
            configure?.Invoke(options);
            options.AddInterceptors(sp.GetRequiredService<TenantDbConnectionInterceptor>());
        });

        return services;
    }

}
