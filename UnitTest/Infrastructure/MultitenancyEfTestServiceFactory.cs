using Jarvis.Caching.Extensions;
using Jarvis.DDD.Domain.DataStorages;
using Jarvis.DDD.Domain.Repositories;
using Jarvis.DDD.Domain.Services;
using Jarvis.ORM.EntityFramework;
using Jarvis.Multitenancy;
using Jarvis.Multitenancy.EntityFramework;
using Jarvis.Multitenancy.EntityFramework.DataStorages;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sample.Multitenancy;
using Sample.Persistence;

namespace UnitTest.Infrastructure;

/// <summary>
/// In-memory DI setup mirroring <c>Sample/HostApplicationBuilderExtension.AddSampleDbContext</c>.
/// </summary>
internal static class MultitenancyEfTestServiceFactory
{
    public static IServiceProvider Create(InMemoryDatabaseRoot? sharedRoot = null)
    {
        var root = sharedRoot ?? new InMemoryDatabaseRoot();
        var builder = Host.CreateApplicationBuilder();
        builder.AddJarvisCaching(o =>
        {
            o.Items["ConnectionString"] = new Jarvis.Caching.CacheEntryOption
            {
                Key = "conn:{dbid}",
                MemSeconds = 3600,
            };
        });
        builder.AddTenantIdResolvers();
        builder.Services.TryAddSingleton<ICurrentTenantAccessor, CurrentTenantAccessor>();
        builder.AddEntityFramework();
        builder.AddMultitenancyEntityFramework();

        var services = builder.Services;
        services.AddHttpContextAccessor();
        services.AddLogging(b => b.SetMinimumLevel(LogLevel.Warning));
        services.AddScoped<IMasterUnitOfWork, MasterUnitOfWork>();
        services.AddScoped<ITenantUnitOfWork, TenantUnitOfWork>();
        services.AddScoped<MultitenancyEfJobRunner>();

        services.AddCoreDbContext<MasterDbContext>(options =>
            options.UseInMemoryDatabase(MultitenancyEfTestDatabaseNames.Master, root));

        services.AddCoreDbContext<TenantDbContext, DbTenantConnectionStringResolver<MasterDbContext, Sample.Entities.Tenant>>(options =>
            options.UseInMemoryDatabase(MultitenancyEfTestDatabaseNames.TenantPlaceholder, root));

        return services.BuildServiceProvider();
    }
}
