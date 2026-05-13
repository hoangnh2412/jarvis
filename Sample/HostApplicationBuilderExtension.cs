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

        // Tenant from header (switch TTenantIdResolver to QueryTenantIdResolver, HostTenantIdResolver, etc.)
        builder.Services.AddCoreDbContext<SampleDbContext, HeaderTenantIdResolver, ConfigConnectionStringResolver>((options, tenantIdResolver, connectionResolver) =>
        {
            var tenantId = tenantIdResolver.GetTenantId();
            if (string.IsNullOrEmpty(tenantId))
                tenantId = nameof(SampleDbContext);

            var connectionString = connectionResolver.GetConnectionString(tenantId);
            options.UseNpgsql(connectionString);
        });

        return builder;
    }
}
