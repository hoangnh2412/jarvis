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
        services.AddCoreDbContext<TenantDbContext, ConfigConnectionStringResolver>((resolver, options) =>
        {
            options.UseNpgsql(resolver.GetConnectionString(nameof(TenantDbContext)));
        });
        services.AddScoped<ITenantUnitOfWork, TenantUnitOfWork>();
        return services;
    }

    public static IServiceCollection AddEFSampleDbContext(this IServiceCollection services)
    {
        services.AddCoreDbContext<SampleDbContext, HttpStorageConnectionStringResolver>((resolver, options) =>
        {
            options.UseNpgsql(resolver.GetConnectionString());
        });
        services.AddScoped<ISampleUnitOfWork, SampleUnitOfWork>();
        return services;
    }
}