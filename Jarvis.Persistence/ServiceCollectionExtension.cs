using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Application.MultiTenancy;
using Jarvis.Persistence.Repositories;
using Jarvis.Shared.Options;
using Jarvis.Persistence.MultiTenancy;

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

        services.AddRepositories();
        services.AddConnectionStringResolver();

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IQueryRepository<>), typeof(BaseQueryRepository<>));
        services.AddScoped(typeof(ICommandRepository<>), typeof(BaseCommandRepository<>));
        services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
        services.AddScoped<Func<string, IStorageContext>>(sp => name => (IStorageContext)sp.GetService(InstanceStorage.StorageContext.InstanceTypes[name]));

        return services;
    }

    private static IServiceCollection AddConnectionStringResolver(this IServiceCollection services)
    {
        services.AddScoped<SingleTenantConnectionStringResolver>();
        services.AddScoped<MultiTenantConnectionStringResolver>();
        services.AddScoped<ITenantIdAccessor, TenantIdAccessor>();
        services.AddScoped<ITenantConnectionAccessor, TenantConnectionAccessor>();

        services.AddScoped<HeaderTenantIdentification>();
        services.AddScoped<QueryTenantIdentification>();
        services.AddScoped<HostTenantIdentification>();
        services.AddSingleton<Func<string, ITenantIdentification>>(sp => name =>
        {
            using (var scope = sp.CreateScope())
            {
                return (ITenantIdentification)sp.GetService(Type.GetType(name));
            }
        });

        services.AddSingleton<Func<string, IConnectionStringResolver>>(sp => name =>
        {
            using (var scope = sp.CreateScope())
            {
                return (IConnectionStringResolver)scope.ServiceProvider.GetService(Type.GetType(name));
            }
        });

        return services;
    }

    /// <summary>
    /// Add DbContext to DI with dynamic ConnectionString by Resolver
    /// </summary>
    /// <param name="services"></param>
    /// <param name="builder"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IServiceCollection AddCoreDbContext<T>(this IServiceCollection services, Action<IConnectionStringResolver, DbContextOptionsBuilder> builder)
        where T : DbContext, IStorageContext
    {
        InstanceStorage.StorageContext.Add<IStorageContext>(typeof(T));
        services.AddScoped(typeof(T));

        services.AddDbContextPool<T>((sp, options) =>
        {
            var factory = sp.GetService<Func<string, IConnectionStringResolver>>();
            var resolver = factory.Invoke(InstanceStorage.ConnectionStringResolver.Get<T>());
            builder.Invoke(resolver, options);
        });

        return services;
    }

    /// <summary>
    /// Add DbContext to DI with connection string
    /// </summary>
    /// <param name="services"></param>
    /// <param name="builder"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IServiceCollection AddCoreDbContext<T>(this IServiceCollection services, Action<string, DbContextOptionsBuilder> builder)
        where T : DbContext, IStorageContext
    {
        InstanceStorage.StorageContext.Add<IStorageContext>(typeof(T));
        services.AddScoped(typeof(T));

        services.AddDbContextPool<T>(async (sp, options) =>
        {
            var factory = sp.GetService<Func<string, IConnectionStringResolver>>();
            var resolver = factory.Invoke(InstanceStorage.ConnectionStringResolver.Get<T>());
            var connection = await resolver.GetConnectionStringAsync(nameof(T));
            builder.Invoke(connection, options);
        });

        return services;
    }
}