using OpenTelemetry.Trace;
using Platform.Authentication;
using Platform.Caching.Redis.Extensions;
using Platform.DDD.Domain;
using Platform.Multitenancy;
using Platform.OpenTelemetry.Abstractions;
using Platform.OpenTelemetry.DDD.Extensions;
using Platform.OpenTelemetry.Extensions;
using Sample.Telemetry;

namespace Sample.Extensions;

public static class SampleOpenTelemetryExtensions
{
    public static WebApplicationBuilder AddSampleOpenTelemetry(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddPlatformOpenTelemetry(builder.Configuration, services =>
            {
                // User/tenant — cả log và trace.
                services.AddUserContextTelemetryEnrichment<CurrentUserInfo, CurrentTenantInfo>();

                // Case 1 — chỉ log.
                services.AddScoped<IEnrichLogService, SampleLogOnlyEnrichmentService>();

                // Case 2 — chỉ trace.
                services.AddScoped<IEnrichTraceService, SampleTraceOnlyEnrichmentService>();

                // Case 3 — cả log và trace (IEnrichmentSource).
                services.AddScoped<IEnrichmentSource, SampleSharedEnrichmentSource>();
            })
            .ConfigureResource()
            .ConfigureLogging()
            .ConfigureTrace(options =>
            {
                options
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddPlatformCachingDistributedRedisInstrumentation(builder.Configuration)
                    .AddPlatformCachingMemoryInvalidationRedisInstrumentation();
            })
            .ConfigureMetric();

        return builder;
    }
}
