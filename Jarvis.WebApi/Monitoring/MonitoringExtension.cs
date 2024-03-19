using OpenTelemetry.Resources;
using System.Reflection;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using System.Diagnostics.Metrics;
using OpenTelemetry.Logs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Jarvis.WebApi.Monitoring;

public static class MonitoringExtension
{
    public static OTLPOption BuildOptionMonitor(this IServiceCollection services, IConfiguration configuration)
    {
        var otlpOptions = new OTLPOption();
        var otlpSection = configuration.GetSection("OTLP");
        services.Configure<OTLPOption>(otlpSection);
        otlpSection.Bind(otlpOptions);

        // OTLPType.TraceInstrumentations.Add(OTLPOption.InstrumentationType.Redis, typeof(RedisTraceInstrumentation).AssemblyQualifiedName);
        // OTLPType.TraceInstrumentations.Add(OTLPOption.InstrumentationType.Elasticsearch, typeof(ElasticsearchTraceInstrumentation).AssemblyQualifiedName);

        // OTLPType.TraceExporters.Add(OTLPOption.ExporterType.Uptrace, typeof(UptraceTraceExporter).AssemblyQualifiedName);
        // OTLPType.MetricExporters.Add(OTLPOption.ExporterType.Uptrace, typeof(UptraceMetricExporter).AssemblyQualifiedName);
        // OTLPType.LoggingExporters.Add(OTLPOption.ExporterType.Uptrace, typeof(UptraceLogExporter).AssemblyQualifiedName);

        return otlpOptions;
    }

    public static IServiceCollection AddCoreMonitor(this IServiceCollection services, OTLPOption otlpOptions)
    {
        services
            .AddOpenTelemetry()
            .ConfigureResource(options =>
            {
                options
                    .AddEnvironmentVariableDetector()
                    .AddTelemetrySdk()
                    .AddService(
                        serviceName: string.IsNullOrEmpty(otlpOptions.ServiceName) ? Assembly.GetEntryAssembly().GetName().Name : otlpOptions.ServiceName,
                        serviceVersion: Assembly.GetEntryAssembly().GetName().Version?.ToString() ?? "unknown",
                        serviceInstanceId: string.IsNullOrEmpty(otlpOptions.ServiceInstanceId) ? Environment.MachineName : otlpOptions.ServiceInstanceId);
            });

        return services;
    }

    public static IServiceCollection AddCoreTrace(this IServiceCollection services, OTLPOption otlpOptions, params string[] sources)
    {
        if (otlpOptions.Tracing == null)
            return services;

        services
            .AddOpenTelemetry()
            .WithTracing(configure =>
            {
                configure.AddSource(Assembly.GetEntryAssembly().GetName().Name);
                if (sources != null && sources.Length > 0)
                    configure.AddSource(sources);

                configure
                    .SetResourceBuilder(BuildResource(otlpOptions))
                    .SetSampler(new AlwaysOnSampler())
                    .SetErrorStatusOnException(true)
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddSqlClientInstrumentation();

                // Intstrumentations
                foreach (var item in OTLPType.TraceInstrumentations)
                {
                    var instance = Activator.CreateInstance(Type.GetType(item.Value), otlpOptions) as ITraceInstrumentation;
                    instance.AddInstrumentation(configure);
                }

                // Exporters
                configure.AddConsoleExporter();
                if (otlpOptions.Tracing.Exporter == default(OTLPOption.ExporterType) || !OTLPType.TraceExporters.TryGetValue(otlpOptions.Tracing.Exporter, out string exporterTypeName))
                    configure.AddConsoleExporter();
                else
                    (Activator.CreateInstance(Type.GetType(exporterTypeName), otlpOptions) as ITraceExporter).AddExporter(configure);
            });

        return services;
    }

    public static IServiceCollection AddCoreMetric(this IServiceCollection services, OTLPOption otlpOptions, params string[] meters)
    {
        if (otlpOptions.Metric == null)
            return services;

        services
            .AddOpenTelemetry()
            .WithMetrics(configure =>
            {
                configure.AddMeter(Assembly.GetEntryAssembly().GetName().Name);
                if (meters != null && meters.Length > 0)
                    configure.AddMeter(meters);

                configure
                    .SetResourceBuilder(BuildResource(otlpOptions))
                    .AddRuntimeInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation();

                switch (otlpOptions.HistogramAggregation)
                {
                    case "exponential":
                        configure.AddView(instrument =>
                        {
                            return instrument.GetType().GetGenericTypeDefinition() == typeof(Histogram<>)
                                ? new Base2ExponentialBucketHistogramConfiguration()
                                : null;
                        });
                        break;
                    default:
                        break;
                }

                // Instrumentations
                foreach (var item in OTLPType.MetricInstrumentations)
                {
                    var instance = Activator.CreateInstance(Type.GetType(item.Value), otlpOptions) as IMetricInstrumentation;
                    instance.AddInstrumentation(configure);
                }

                // Exporters
                if (otlpOptions.Metric.Exporter == default(OTLPOption.ExporterType) || !OTLPType.MetricExporters.TryGetValue(otlpOptions.Metric.Exporter, out string exporterTypeName))
                    configure.AddConsoleExporter();
                else
                    (Activator.CreateInstance(Type.GetType(exporterTypeName), otlpOptions) as IMetricExporter).AddExporter(configure);
            });

        return services;
    }

    public static IServiceCollection AddCoreLogging(this IServiceCollection services, OTLPOption otlpOptions)
    {
        services.AddLogging(builder =>
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

                options.SetResourceBuilder(BuildResource(otlpOptions));

                // Exporters
                options.AddConsoleExporter();
                if (otlpOptions.Logging.Exporter == default(OTLPOption.ExporterType) || !OTLPType.LoggingExporters.TryGetValue(otlpOptions.Logging.Exporter, out string exporterTypeName))
                    options.AddConsoleExporter();
                else
                    (Activator.CreateInstance(Type.GetType(exporterTypeName), otlpOptions) as ILoggingExporter).AddExporter(options);
            });
        });

        return services;
    }

    private static ResourceBuilder BuildResource(OTLPOption otlpOptions)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (string.IsNullOrEmpty(env))
            env = "Development";

        return ResourceBuilder
            .CreateDefault()
            .AddEnvironmentVariableDetector()
            .AddTelemetrySdk()
            .AddAttributes(new List<KeyValuePair<string, object>> {
                new KeyValuePair<string, object>("deployment.environment", env)
            })
            .AddService(
                serviceName: string.IsNullOrEmpty(otlpOptions.ServiceName) ? Assembly.GetEntryAssembly().GetName().Name : otlpOptions.ServiceName,
                serviceVersion: Assembly.GetEntryAssembly().GetName().Version?.ToString() ?? "unknown",
                serviceInstanceId: string.IsNullOrEmpty(otlpOptions.ServiceInstanceId) ? Environment.MachineName : otlpOptions.ServiceInstanceId);
    }
}