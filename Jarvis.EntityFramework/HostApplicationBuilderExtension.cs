using Jarvis.Domain.DataStorages;
using Jarvis.Domain.Repositories;
using Jarvis.EntityFramework.DataStorages;
using Jarvis.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;
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

    public static IHostApplicationBuilder AddMultitenancy(this IHostApplicationBuilder builder)
    {
        builder.Services.AddKeyedScoped<ITenantIdResolver, HeaderTenantIdResolver>(nameof(HeaderTenantIdResolver));
        builder.Services.AddKeyedScoped<ITenantIdResolver, QueryTenantIdResolver>(nameof(QueryTenantIdResolver));
        builder.Services.AddKeyedScoped<ITenantIdResolver, UserTenantIdResolver>(nameof(UserTenantIdResolver));
        builder.Services.AddKeyedScoped<ITenantConnectionStringResolver, ConfigConnectionStringResolver>(nameof(ConfigConnectionStringResolver));
        return builder;
    }

    /// <summary>
    /// Add DbContext to DI with dynamic ConnectionString by Resolver
    /// </summary>
    /// <param name="services"></param>
    /// <param name="builder"></param>
    /// <typeparam name="TDbContext"></typeparam>
    /// <typeparam name="TTenantIdResolver"></typeparam>
    /// <typeparam name="TConnectionStringResolver"></typeparam>
    /// <returns></returns>
    public static IServiceCollection AddCoreDbContext<TDbContext, TTenantIdResolver, TConnectionStringResolver>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder, ITenantIdResolver, ITenantConnectionStringResolver> builder)
        where TDbContext : BaseStorageContext<TDbContext>
        where TTenantIdResolver : ITenantIdResolver
        where TConnectionStringResolver : ITenantConnectionStringResolver
    {
        services.AddScoped<IDbContextFactory<TDbContext>, BaseDbContextFactory<TDbContext, TTenantIdResolver, TConnectionStringResolver>>(sp => new BaseDbContextFactory<TDbContext, TTenantIdResolver, TConnectionStringResolver>(sp, builder));
        services.AddPooledDbContextFactory<TDbContext>((sp, options) =>
        {
            using var scope = sp.CreateScope();
            builder.Invoke(
                options,
                scope.ServiceProvider.GetRequiredKeyedService<ITenantIdResolver>(typeof(TTenantIdResolver).Name),
                scope.ServiceProvider.GetRequiredKeyedService<ITenantConnectionStringResolver>(typeof(TConnectionStringResolver).Name));
        });
        return services;
    }

    public static IServiceCollection AddCoreDbContext<TDbContext>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder, string> builder)
        where TDbContext : BaseStorageContext<TDbContext>
    {
        services.AddScoped<IDbContextFactory<TDbContext>, BaseDbContextFactory<TDbContext, HeaderTenantIdResolver, ConfigConnectionStringResolver>>(sp =>
        {
            return new BaseDbContextFactory<TDbContext, HeaderTenantIdResolver, ConfigConnectionStringResolver>(sp, async (options, tenantIdResolver, connectionResolver) =>
            {
                using var scope = sp.CreateScope();
                var tenantId = await tenantIdResolver.GetTenantIdAsync();
                if (string.IsNullOrEmpty(tenantId))
                    tenantId = typeof(TDbContext).Name;

                var connectionString = await connectionResolver.GetConnectionStringAsync(tenantId);
                builder.Invoke(options, connectionString);
            });
        });

        services.AddPooledDbContextFactory<TDbContext>(async (sp, options) =>
        {
            using var scope = sp.CreateScope();
            var tenantId = await scope.ServiceProvider.GetRequiredKeyedService<ITenantIdResolver>(typeof(HeaderTenantIdResolver).Name).GetTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId))
                tenantId = typeof(TDbContext).Name;
            var connectionString = await scope.ServiceProvider.GetRequiredKeyedService<ITenantConnectionStringResolver>(typeof(ConfigConnectionStringResolver).Name).GetConnectionStringAsync(tenantId);
            builder.Invoke(options, connectionString);
        });
        return services;
    }
}