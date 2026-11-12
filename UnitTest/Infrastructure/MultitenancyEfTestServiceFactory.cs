using Platform.Caching.Extensions;
using Platform.DDD.Domain.DataStorages;
using Platform.DDD.Domain.Repositories;
using Platform.DDD.Domain.Services;
using Platform.ORM.EntityFramework;
using Platform.Multitenancy;
using Platform.Multitenancy.EntityFramework;
using Platform.Multitenancy.EntityFramework.DataStorages;
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
/// In-memory DI setup mirroring <c>Sample/Extensions/SampleDbContextExtensions.AddSampleDbContext</c>.
/// </summary>
// CROSS-MODULE 2026-11-03 · slice Sample composition root
// Nguyên nhân: AddSampleDbContext chuyển khỏi Sample/HostApplicationBuilderExtension.cs.
// Lý do: sửa đường dẫn trong comment cho khớp file mới, không đổi factory.
// Owner: UnitTest. Không đụng logic Create.
internal static class MultitenancyEfTestServiceFactory
{
    public static IServiceProvider Create(InMemoryDatabaseRoot? sharedRoot = null)
    {
        var root = sharedRoot ?? new InMemoryDatabaseRoot();
        var builder = Host.CreateApplicationBuilder();
        builder.AddPlatformCaching(o =>
        {
            o.Items["ConnectionString"] = new Platform.Caching.CacheEntryOption
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
        services.AddScoped<ISampleMasterUnitOfWork, SampleMasterUnitOfWork>();
        services.AddScoped<ISampleTenantUnitOfWork, SampleTenantUnitOfWork>();
        services.AddScoped<MultitenancyEfJobRunner>();

        services.AddCoreDbContext<SampleMasterDbContext>(options =>
            options.UseInMemoryDatabase(MultitenancyEfTestDatabaseNames.Master, root));

        services.AddCoreDbContext<SampleTenantDbContext, DbTenantConnectionStringResolver<SampleMasterDbContext, Sample.Entities.Tenant>>(options =>
            options.UseInMemoryDatabase(MultitenancyEfTestDatabaseNames.TenantPlaceholder, root));

        return services.BuildServiceProvider();
    }
}
