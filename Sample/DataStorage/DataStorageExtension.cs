using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Jarvis.Persistence;
using Jarvis.Persistence.DataContexts;
using Jarvis.Persistence.MultiTenancy;
using Jarvis.Application.MultiTenancy;

namespace Sample.DataStorage;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddMultiTenancy(this IServiceCollection services)
    {
        services.AddScoped<SingleTenantConnectionStringResolver>();
        services.AddScoped<MultiTenantConnectionStringResolver>();
        services.AddScoped<HostTenantIdentification>();
        services.AddScoped<ITenantIdAccessor, TenantIdAccessor>();
        services.AddScoped<ITenantConnectionAccessor, TenantConnectionAccessor>();
        return services;
    }

    public static IServiceCollection AddTenantDbContext(this IServiceCollection services)
    {
        // services.AddPooledDbContextFactory<TenantDbContext>((sp, options) =>
        // {
        //     var configuration = sp.GetService<IConfiguration>();
        //     var conn = configuration.GetConnectionString(nameof(TenantDbContext));
        //     options.UseNpgsql(conn);
        // });

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