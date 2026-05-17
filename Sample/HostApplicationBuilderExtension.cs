using Jarvis.Domain.DataStorages;
using Jarvis.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Sample.Persistence;

namespace Sample;

public static class HostApplicationBuilderExtension
{
    public static IHostApplicationBuilder AddSampleDbContext(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<ISampleUnitOfWork, SampleUnitOfWork>();

        builder.Services.AddKeyedConfigConnectionStringResolver();

        // Connection string resolved on open via TenantDbConnectionInterceptor + ITenantIdResolverFactory
        builder.Services.AddCoreDbContext<SampleDbContext, ConfigConnectionStringResolver>(options =>
            options.UseNpgsql("Host=localhost;Database=placeholder"));

        return builder;
    }
}
