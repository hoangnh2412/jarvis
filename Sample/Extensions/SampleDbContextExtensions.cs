using Platform.Authorization.EntityFramework.Persistence;
using Platform.Host.Persistence;
using Platform.Host.Postgresql.Extensions;
using Platform.Multitenancy.EntityFramework;
using Platform.ORM.EntityFramework;
using Sample.Multitenancy;
using Sample.Persistence;

namespace Sample.Extensions;

public static class SampleDbContextExtensions
{
    public const string MasterConnectionStringName = "MasterDbContext";

    public const string TenantConnectionStringName = "TenantDbContext";

    public static IHostApplicationBuilder AddSampleDbContext(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<ISampleMasterUnitOfWork, SampleMasterUnitOfWork>();
        builder.Services.AddScoped<ISampleTenantUnitOfWork, SampleTenantUnitOfWork>();
        builder.Services.AddScoped<IPlatformIdentityStore>(sp => new PlatformPlatformIdentityStore(sp.GetRequiredService<SampleMasterDbContext>()));

        builder.Services.AddCoreDbContext<SampleMasterDbContext>(options =>
            options.UsePlatformHostPostgreSql(
                builder.Configuration.GetConnectionString(MasterConnectionStringName)));

        // TEMP: bypass SampleMaster.Tenant lookup — see DevBypassTenantConnectionStringResolver.
        // builder.Services.AddCoreDbContext<SampleTenantDbContext, DbTenantConnectionStringResolver<SampleMasterDbContext, Tenant>>(options =>
        //     options.UseNpgsql(builder.Configuration.GetConnectionString("TenantDbContext")));
        builder.Services.AddCoreDbContext<SampleTenantDbContext, DevBypassTenantConnectionStringResolver>(options =>
            options.UsePlatformHostPostgreSql(
                builder.Configuration.GetConnectionString(TenantConnectionStringName)));

        return builder;
    }
}
