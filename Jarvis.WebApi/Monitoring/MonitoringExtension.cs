using Microsoft.Extensions.Configuration;
using Jarvis.Shared.Options;
using OpenTelemetry.Resources;
using System.Reflection;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using System.Diagnostics.Metrics;
using OpenTelemetry.Logs;
using Jarvis.Application.MultiTenancy;
using Jarvis.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Jarvis.Application.Interfaces.Repositories;

namespace Jarvis.WebApi.Monitoring;

public static class MonitoringExtension
{
    public static IServiceCollection AddCoreMonitor(this IServiceCollection services, IConfiguration configuration)
    {
        var otlpOptions = new OTLPOption();
        var otlpSection = configuration.GetSection("OTLP");
        services.Configure<OTLPOption>(otlpSection);
        otlpSection.Bind(otlpOptions);

        services.AddOpenTelemetry()
            .ConfigureResource(options =>
            {
                options.AddService(
                    serviceName: string.IsNullOrEmpty(otlpOptions.ServiceName) ? Assembly.GetEntryAssembly().GetName().Name : otlpOptions.ServiceName,
                    serviceVersion: Assembly.GetEntryAssembly().GetName().Version?.ToString() ?? "unknown",
                    serviceInstanceId: string.IsNullOrEmpty(otlpOptions.ServiceInstanceId) ? Environment.MachineName : otlpOptions.ServiceInstanceId);
            })
            .WithTracing(builder =>
            {
                if (otlpOptions.Tracing == null)
                    return;

                builder
                    .AddSource(Assembly.GetEntryAssembly().GetName().Name)
                    .SetSampler(new AlwaysOnSampler())
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        // options.EnrichWithHttpRequest = (activity, httpRequest) =>
                        // {
                        //     activity.SetParentId("d43431c6d79e8a26");
                        // };
                        options.RecordException = otlpOptions.AspNetCoreInstrumentation == null ? true : otlpOptions.AspNetCoreInstrumentation.RecordException;
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })
                    // TODO: Implement config
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddRedisInstrumentation()
                    .AddElasticsearchClientInstrumentation()
                    .AddSqlClientInstrumentation();

                switch (otlpOptions.Tracing.Exporter)
                {
                    case OTLPOption.ExporterType.OTLP:
                        builder.AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri(otlpOptions.Tracing.Endpoint);
                        });
                        break;
                    default:
                        builder.AddConsoleExporter();
                        break;
                }
            })
            .WithMetrics(builder =>
            {
                builder
                    .AddMeter(Assembly.GetEntryAssembly().GetName().Name)
                    .AddRuntimeInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation();

                switch (otlpOptions.HistogramAggregation)
                {
                    case "exponential":
                        builder.AddView(instrument =>
                        {
                            return instrument.GetType().GetGenericTypeDefinition() == typeof(Histogram<>)
                                ? new Base2ExponentialBucketHistogramConfiguration()
                                : null;
                        });
                        break;
                    default:
                        break;
                }

                switch (otlpOptions.Metric.Exporter)
                {
                    case OTLPOption.ExporterType.Prometheus:
                        builder.AddPrometheusExporter();
                        break;
                    case OTLPOption.ExporterType.OTLP:
                        builder.AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri(otlpOptions.Metric.Endpoint);
                        });
                        break;
                    default:
                        builder.AddConsoleExporter();
                        break;
                }
            });

        return services;
    }

    public static IServiceCollection AddCoreLogging(this IServiceCollection services, IConfiguration configuration)
    {
        var otlpOptions = new OTLPOption();
        configuration.GetSection("OTLP").Bind(otlpOptions);

        services.AddLogging((builder) =>
        {
            builder.ClearProviders();
            builder.Configure(options =>
            {
                options.ActivityTrackingOptions = ActivityTrackingOptions.TraceId | ActivityTrackingOptions.SpanId;
            });
            builder.AddOpenTelemetry(options =>
            {
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;
                options.ParseStateValues = true;

                options
                    .SetResourceBuilder(
                        ResourceBuilder
                        .CreateDefault()
                        .AddService(
                            serviceName: string.IsNullOrEmpty(otlpOptions.ServiceName) ? Assembly.GetEntryAssembly().GetName().Name : otlpOptions.ServiceName,
                            serviceVersion: Assembly.GetEntryAssembly().GetName().Version?.ToString() ?? "unknown",
                            serviceInstanceId: string.IsNullOrEmpty(otlpOptions.ServiceInstanceId) ? Environment.MachineName : otlpOptions.ServiceInstanceId)
                    );

                switch (otlpOptions.Logging.Exporter)
                {
                    case OTLPOption.ExporterType.OTLP:
                        options.AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri(otlpOptions.Logging.Endpoint);
                        });
                        break;
                    default:
                        options.AddConsoleExporter();
                        break;
                }
            });
        });

        return services;
    }

    public static IServiceCollection AddCoreHealthcheckMySql<T>(this IServiceCollection services) where T : IStorageContext
    {
        services
            .AddHealthChecks()
            .AddMySql(sp =>
            {
                var factory = sp.GetService<Func<string, IConnectionStringResolver>>();
                var resolver = factory.Invoke(InstanceStorage.ConnectionStringResolver);

                return resolver.GetConnectionStringAsync(typeof(T).Name).GetAwaiter().GetResult();
            });

        return services;
    }

    public static IServiceCollection AddCoreHealthcheck(this IServiceCollection services)
    {
        services.AddHealthChecks();
        return services;
    }
}