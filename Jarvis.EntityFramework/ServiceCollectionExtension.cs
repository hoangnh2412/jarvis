using Jarvis.Domain.DataStorages;
using Jarvis.Domain.Repositories;
using Jarvis.EntityFramework.DataStorages;
using Jarvis.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.EntityFramework;

public static class ServiceCollectionExtension
{
    /// <summary>
    /// Register base repositories for Entify Framework
    /// </summary>
    /// <param name="services"></param>
    public static IServiceCollection AddCorePersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var otlpOptions = new StorageContextOption();
        var otlpSection = configuration.GetSection("StorageContext");
        services.Configure<StorageContextOption>(otlpSection);

        services.AddRepositories();
        services.AddMultitenancy();

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IQueryRepository<>), typeof(BaseQueryRepository<>));
        services.AddScoped(typeof(ICommandRepository<>), typeof(BaseCommandRepository<>));
        services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));

        return services;
    }

    private static IServiceCollection AddMultitenancy(this IServiceCollection services)
    {
        services.AddKeyedScoped<ITenantIdResolver, HeaderTenantIdResolver>(nameof(HeaderTenantIdResolver));
        services.AddKeyedScoped<ITenantIdResolver, QueryTenantIdResolver>(nameof(QueryTenantIdResolver));
        services.AddKeyedScoped<ITenantIdResolver, UserTenantIdResolver>(nameof(UserTenantIdResolver));

        services.AddKeyedScoped<ITenantConnectionStringResolver, ConfigConnectionStringResolver>(nameof(ConfigConnectionStringResolver));

        return services;
    }

    /// <summary>
    /// Add DbContext to DI with dynamic ConnectionString by Resolver
    /// </summary>
    /// <param name="services"></param>
    /// <param name="builder"></param>
    /// <typeparam name="TDbContext"></typeparam>
    /// <typeparam name="TFactory"></typeparam>
    /// <typeparam name="TResolver"></typeparam>
    /// <returns></returns>
    public static IServiceCollection AddCoreDbContext<TDbContext, TResolver, TFactory>(this IServiceCollection services, Action<TResolver, DbContextOptionsBuilder> builder)
     where TDbContext : BaseStorageContext<TDbContext>
     where TFactory : BaseDbContextFactory<TDbContext>
     where TResolver : ITenantConnectionStringResolver
    {
        services.AddScoped<TFactory>();
        services.AddPooledDbContextFactory<TDbContext>((sp, options) =>
        {
            var resolver = sp.GetRequiredKeyedService<TResolver>(typeof(TResolver).Name);
            builder.Invoke(resolver, options);
        });
        return services;
    }
}