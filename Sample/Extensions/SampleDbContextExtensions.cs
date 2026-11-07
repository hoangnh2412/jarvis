using Platform.Host.Postgresql.Extensions;
using Platform.Multitenancy.EntityFramework;
using Platform.Multitenancy.EntityFramework.DataStorages;
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
        builder.Services.AddScoped<IMasterUnitOfWork, MasterUnitOfWork>();
        builder.Services.AddScoped<ITenantUnitOfWork, TenantUnitOfWork>();

        builder.Services.AddCoreDbContext<MasterDbContext>(options =>
            options.UsePlatformHostPostgreSql(
                builder.Configuration.GetConnectionString(MasterConnectionStringName)));

        // TEMP: bypass SampleMaster.Tenant lookup — see DevBypassTenantConnectionStringResolver.
        // builder.Services.AddCoreDbContext<TenantDbContext, DbTenantConnectionStringResolver<MasterDbContext, Tenant>>(options =>
        //     options.UseNpgsql(builder.Configuration.GetConnectionString("TenantDbContext")));
        builder.Services.AddCoreDbContext<TenantDbContext, DevBypassTenantConnectionStringResolver>(options =>
            options.UsePlatformHostPostgreSql(
                builder.Configuration.GetConnectionString(TenantConnectionStringName)));

        return builder;
    }
}
