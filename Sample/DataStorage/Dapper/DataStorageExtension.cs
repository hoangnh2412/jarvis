using Jarvis.Persistence;
using Jarvis.Application.MultiTenancy;
using Jarvis.Shared.DependencyInjection;
using Jarvis.Application.Interfaces.Repositories;
using Npgsql;

namespace Sample.DataStorage.Dapper;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddDapperMultiTenancy(this IServiceCollection services)
    {
        // services.AddScopedByName<ITenantIdResolver, HostTenantIdResolver>();

        // services.AddScopedByName<ITenantConnectionStringResolver, HttpStorageConnectionStringResolver>();
        // services.AddScopedByName<ITenantConnectionStringResolver, StorageConnectionStringResolver>();
        return services;
    }

    public static IServiceCollection AddDapperTenantDbContext(this IServiceCollection services)
    {
        services.AddScoped<ITenantUnitOfWork, TenantUnitOfWork>();
        services.AddScoped<IStorageContext, TenantDbContext>((sp) =>
        {
            var resolver = sp.GetService<ConfigConnectionStringResolver>();
            var connectionString = resolver.GetConnectionString(nameof(TenantDbContext));

            var connection = new NpgsqlConnection(connectionString);

            return new TenantDbContext(connection);
        });

        // services.AddCoreDbContext<TenantDbContext, ConfigConnectionStringResolver>((resolver, options) =>
        // {
        //     options.UseNpgsql(resolver.GetConnectionString(nameof(TenantDbContext)));
        // });
        return services;
    }

    // public static IServiceCollection AddDapperSampleDbContext(this IServiceCollection services)
    // {
    //     services.AddScoped<ISampleUnitOfWork, SampleUnitOfWork>();
    //     services.AddSingleton<IDbContextFactory<SampleDbContext>, ScopedDbContextFactory<SampleDbContext, ConfigConnectionStringResolver>>();
    //     services.AddCoreDbContext<SampleDbContext, HttpStorageConnectionStringResolver>((resolver, options) =>
    //     {
    //         options.UseNpgsql(resolver.GetConnectionString());
    //     });

    //     return services;
    // }
}