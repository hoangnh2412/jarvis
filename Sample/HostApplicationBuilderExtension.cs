using Jarvis.Domain.DataStorages;
using Jarvis.EntityFramework;
using Jarvis.EntityFramework.DataStorages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sample.Entities;
using Sample.Multitenancy;
using Sample.Persistence;

namespace Sample;

public static class HostApplicationBuilderExtension
{
    public static IHostApplicationBuilder AddSampleDbContext(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<JobTenantContext>();
        builder.Services.AddKeyedScoped<ITenantIdResolver, JobTenantIdResolver>(nameof(JobTenantIdResolver));
        builder.Services.Replace(ServiceDescriptor.Scoped<ITenantIdResolverFactory, SampleTenantIdResolverFactory>());

        builder.Services.AddScoped<IMasterUnitOfWork, MasterUnitOfWork>();
        builder.Services.AddScoped<ISampleUnitOfWork, SampleUnitOfWork>();
        builder.Services.AddScoped<TenantEfTestService>();

        builder.Services.AddCoreDbContext<MasterDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("MasterDbContext")));

        builder.Services.AddCoreDbContext<TenantDbContext, DbTenantConnectionStringResolver<MasterDbContext, Tenant>>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("TenantDbContext")!));

        return builder;
    }
}
