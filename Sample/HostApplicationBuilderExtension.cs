using Jarvis.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Sample.Persistence;

namespace Sample;

public static class HostApplicationBuilderExtension
{
    public static IHostApplicationBuilder AddSampleDbContext(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<ISampleUnitOfWork, SampleUnitOfWork>();

        // Basic
        builder.Services.AddCoreDbContext<SampleDbContext>((options, connectionString) => options.UseNpgsql(connectionString));

        // Advance
        // builder.Services.AddCoreDbContext<SampleDbContext, HeaderTenantIdResolver, ConfigConnectionStringResolver>(async (options, tenantIdResolver, connectionResolver) =>
        // {
        //     var tenantId = await tenantIdResolver.GetTenantIdAsync() ?? nameof(SampleDbContext);
        //     var connectionString = await connectionResolver.GetConnectionStringAsync(tenantId);
        //     options.UseNpgsql(connectionString);
        // });
        return builder;
    }
}