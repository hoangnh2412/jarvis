using Jarvis.Domain.DataStorages;
using Jarvis.Domain.Repositories;
using Jarvis.EntityFramework;
using Jarvis.EntityFramework.DataStorages;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
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
        builder.AddEntityFramework();

        var services = builder.Services;
        services.AddHttpContextAccessor();
        services.AddLogging(b => b.SetMinimumLevel(LogLevel.Warning));
        services.AddScoped<IMasterUnitOfWork, MasterUnitOfWork>();
        services.AddScoped<ISampleUnitOfWork, SampleUnitOfWork>();
        services.AddScoped<MultitenancyEfJobRunner>();

        services.AddCoreDbContext<MasterDbContext>(options =>
            options.UseInMemoryDatabase(MultitenancyEfTestDatabaseNames.Master, root));

        services.AddCoreDbContext<TenantDbContext, DbTenantConnectionStringResolver<MasterDbContext, Sample.Entities.Tenant>>(options =>
            options.UseInMemoryDatabase(MultitenancyEfTestDatabaseNames.TenantPlaceholder, root));

        return services.BuildServiceProvider();
    }
}
