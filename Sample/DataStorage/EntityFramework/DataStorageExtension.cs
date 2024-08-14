using Microsoft.EntityFrameworkCore;
using Jarvis.Persistence;
using Jarvis.Application.MultiTenancy;
using Jarvis.Shared.DependencyInjection;

namespace Sample.DataStorage.EntityFramework;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddEFMultiTenancy(this IServiceCollection services)
    {
        services.AddScopedByName<ITenantIdResolver, HostTenantIdResolver>();

        services.AddScopedByName<ITenantConnectionStringResolver, HttpStorageConnectionStringResolver>();
        services.AddScopedByName<ITenantConnectionStringResolver, StorageConnectionStringResolver>();
        return services;
    }

    public static IServiceCollection AddEFTenantDbContext(this IServiceCollection services)
    {
        services.AddScoped<ITenantUnitOfWork, TenantUnitOfWork>();
        services.AddCoreDbContext<TenantDbContext, ConfigConnectionStringResolver>((resolver, options) =>
        {
            options.UseNpgsql(resolver.GetConnectionString(nameof(TenantDbContext)));
        });
        return services;
    }

    public static IServiceCollection AddEFSampleDbContext(this IServiceCollection services)
    {
        services.AddScoped<ISampleUnitOfWork, SampleUnitOfWork>();
        services.AddSingleton<IDbContextFactory<SampleDbContext>, ScopedDbContextFactory<SampleDbContext, ConfigConnectionStringResolver>>();
        services.AddCoreDbContext<SampleDbContext, HttpStorageConnectionStringResolver>((resolver, options) =>
        {
            options.UseNpgsql(resolver.GetConnectionString());
        });

        return services;
    }
}