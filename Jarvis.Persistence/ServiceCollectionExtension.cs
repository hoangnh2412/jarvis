using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Application.MultiTenancy;
using Jarvis.Shared.Options;
using Jarvis.Shared.DependencyInjection;

namespace Jarvis.Persistence;

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

        services.AddScopedByName<ITenantIdResolver, HeaderTenantIdResolver>();
        services.AddScopedByName<ITenantIdResolver, QueryTenantIdResolver>();
        services.AddScopedByName<ITenantIdResolver, UserTenantIdResolver>();

        services.AddScopedByName<ITenantConnectionStringResolver, ConfigConnectionStringResolver>();

        return services;
    }

    public static IServiceCollection AddEFRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IQueryRepository<>), typeof(Jarvis.Persistence.Repositories.EntityFramework.BaseQueryRepository<>));
        services.AddScoped(typeof(ICommandRepository<>), typeof(Jarvis.Persistence.Repositories.EntityFramework.BaseCommandRepository<>));
        services.AddScoped(typeof(IEFRepository<>), typeof(Jarvis.Persistence.Repositories.EntityFramework.BaseRepository<>));

        return services;
    }
    
    /// <summary>
    /// Add DbContext to DI with dynamic ConnectionString by Resolver
    /// </summary>
    /// <param name="services"></param>
    /// <param name="builder"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResolver"></typeparam>
    /// <returns></returns>
    public static IServiceCollection AddCoreDbContext<T, TResolver>(this IServiceCollection services, Action<ITenantConnectionStringResolver, DbContextOptionsBuilder> builder)
        where T : DbContext, IStorageContext
        where TResolver : ITenantConnectionStringResolver
    {
        InstanceStorage.DbContexts.Add(typeof(T).AssemblyQualifiedName, typeof(TResolver).AssemblyQualifiedName);
        services.AddPooledDbContextFactory<T>((sp, options) =>
        {
            using (var scope = sp.CreateScope())
            {
                var resolver = scope.ServiceProvider.GetService<ITenantConnectionStringResolver>(typeof(TResolver).Name);
                builder.Invoke(resolver, options);
            }
        });
        return services;
    }
}