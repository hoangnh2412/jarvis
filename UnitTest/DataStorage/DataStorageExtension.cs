using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Jarvis.Persistence;
using Jarvis.Persistence.DataContexts;
using Jarvis.Persistence.MultiTenancy;

namespace UnitTest.DataStorage;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddTenantDbContext(this IServiceCollection services)
    {
        InstanceStorage.ConnectionStringResolver.Set<TenantDbContext, SingleTenantConnectionStringResolver>();
        services.AddCoreDbContext<TenantDbContext>((connection, options) => options.UseNpgsql(connection));
        services.AddScoped<ITenantUnitOfWork, TenantUnitOfWork>();
        return services;
    }

    public static IServiceCollection AddSampleDbContext(this IServiceCollection services)
    {
        InstanceStorage.ConnectionStringResolver.Set<SampleDbContext, MultiTenantConnectionStringResolver>();
        services.AddCoreDbContext<SampleDbContext>((connection, options) => options.UseNpgsql(connection));
        services.AddScoped<ISampleUnitOfWork, SampleUnitOfWork>();
        return services;
    }
}