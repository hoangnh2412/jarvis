using Jarvis.Domain.DataStorages;
using Jarvis.EntityFramework;
using Jarvis.EntityFramework.DataStorages;
using Microsoft.EntityFrameworkCore;
using Sample.Persistence;

namespace Sample;

public static class HostApplicationBuilderExtension
{
    public static IHostApplicationBuilder AddSampleDbContext(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IMasterUnitOfWork, MasterUnitOfWork>();
        builder.Services.AddScoped<ISampleUnitOfWork, SampleUnitOfWork>();

        builder.Services.AddCoreDbContext<MasterDbContext, ConfigConnectionStringResolver>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("MasterDbContext")));

        builder.Services.AddCoreDbContext<TenantDbContext, DbTenantConnectionStringResolver<MasterDbContext>>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("TenantDbContext")));

        return builder;
    }
}
