using Platform.ORM.EntityFramework;
using Platform.Multitenancy.EntityFramework;
using Platform.Multitenancy.EntityFramework.DataStorages;
using Microsoft.EntityFrameworkCore;
using Sample.Multitenancy;
using Sample.Persistence;

namespace Sample;

public static class HostApplicationBuilderExtension
{
    public static IHostApplicationBuilder AddSampleDbContext(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IMasterUnitOfWork, MasterUnitOfWork>();
        builder.Services.AddScoped<ITenantUnitOfWork, TenantUnitOfWork>();

        builder.Services.AddCoreDbContext<MasterDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("MasterDbContext")));

        // TEMP: bypass SampleMaster.Tenant lookup — see DevBypassTenantConnectionStringResolver.
        // builder.Services.AddCoreDbContext<TenantDbContext, DbTenantConnectionStringResolver<MasterDbContext, Tenant>>(options =>
        //     options.UseNpgsql(builder.Configuration.GetConnectionString("TenantDbContext")));
        builder.Services.AddCoreDbContext<TenantDbContext, DevBypassTenantConnectionStringResolver>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("TenantDbContext")));

        return builder;
    }
}
