using Jarvis.ORM.EntityFramework;
using Jarvis.Multitenancy.EntityFramework;
using Jarvis.Multitenancy.EntityFramework.DataStorages;
using Microsoft.EntityFrameworkCore;
using Sample.Entities;
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

        builder.Services.AddCoreDbContext<TenantDbContext, DbTenantConnectionStringResolver<MasterDbContext, Tenant>>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("TenantDbContext")));

        return builder;
    }
}
