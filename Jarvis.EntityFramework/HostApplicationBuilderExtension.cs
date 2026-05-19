using Jarvis.Domain.DataStorages;
using Jarvis.Domain.Repositories;
using Jarvis.EntityFramework.DataStorages;
using Jarvis.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;
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
        builder.Services.TryAddKeyedScoped<ITenantConnectionStringResolver, ConfigConnectionStringResolver>(nameof(ConfigConnectionStringResolver));
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
        builder.Services.TryAddKeyedScoped<ITenantIdResolver, HeaderTenantIdResolver>(nameof(HeaderTenantIdResolver));
        builder.Services.TryAddKeyedScoped<ITenantIdResolver, QueryTenantIdResolver>(nameof(QueryTenantIdResolver));
        builder.Services.TryAddKeyedScoped<ITenantIdResolver, UserTenantIdResolver>(nameof(UserTenantIdResolver));
        builder.Services.TryAddKeyedScoped<ITenantIdResolver, HostTenantIdResolver>(nameof(HostTenantIdResolver));
        builder.Services.TryAddScoped<ITenantIdResolverFactory, TenantIdResolverFactory>();
        return builder;
    }

    /// <summary>
    /// Registers <see cref="IDbContextFactory{TContext}"/> with <see cref="TenantDbConnectionInterceptor"/>,
    /// keyed <see cref="ITenantConnectionStringResolver"/> for <typeparamref name="TDbContext"/>,
    /// and <see cref="ITenantConnectionStringResolverFactory"/>.
    /// Tenant id is resolved via <see cref="ITenantIdResolverFactory"/> (header, user, query, host) when the connection opens.
    /// </summary>
    /// <typeparam name="TDbContext">Concrete <see cref="DbContext"/> deriving from <see cref="BaseStorageContext{TDbContext}"/>.</typeparam>
    /// <typeparam name="TConnectionStringResolver">Keyed <see cref="ITenantConnectionStringResolver"/> (key = <typeparamref name="TDbContext"/> type name).</typeparam>
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
        services.TryAddKeyedScoped<ITenantConnectionStringResolver, TConnectionStringResolver>(typeof(TDbContext).Name);
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
    /// Registers <see cref="IDbContextFactory{TContext}"/> with <see cref="TenantDbConnectionInterceptor"/>.
    /// Requires a keyed <see cref="ITenantConnectionStringResolver"/> registered for <typeparamref name="TDbContext"/> (see the two-type-parameter overload).
    /// Tenant id is resolved via <see cref="ITenantIdResolverFactory"/> when the connection opens.
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
        services.TryAddKeyedScoped<ITenantConnectionStringResolver, ConfigConnectionStringResolver>(typeof(TDbContext).Name);
        services.TryAddScoped<ITenantConnectionStringResolverFactory, TenantConnectionStringResolverFactory>();

        services.AddDbContextFactory<TDbContext>((sp, options) =>
        {
            configure?.Invoke(options);
        });

        return services;
    }

}
