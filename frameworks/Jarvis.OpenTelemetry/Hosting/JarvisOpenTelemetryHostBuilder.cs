using System.Diagnostics.Metrics;
using System.Reflection;
using Jarvis.OpenTelemetry.Abstractions;
using Jarvis.OpenTelemetry.Configuration;
using Jarvis.OpenTelemetry.Instrumentations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Jarvis.OpenTelemetry.Hosting;

/// <summary>
/// Fluent registration for Jarvis OpenTelemetry defaults on top of <see cref="OpenTelemetryBuilder"/>.
/// </summary>
public sealed class JarvisOpenTelemetryHostBuilder
{
    private readonly IServiceCollection _services;
    private readonly JarvisOpenTelemetryOptions _options;
    private readonly IConfigurationSection _otelSection;
    private readonly OpenTelemetryBuilder _builder;

    public JarvisOpenTelemetryHostBuilder(
        IServiceCollection services,
        JarvisOpenTelemetryOptions options,
        IConfigurationSection otelConfigurationSection)
    {
        _services = services;
        _options = options;
        _otelSection = otelConfigurationSection;
        _builder = _services.AddOpenTelemetry();

        _services.ConfigureOpenTelemetryTracerProvider((sp, builder) =>
        {
            foreach (var instrumentation in sp.GetServices<ITraceInstrumentation>())
                instrumentation.AddInstrumentation(sp, builder);
            foreach (var exporter in sp.GetServices<ITraceExporter>())
                exporter.AddExporter(builder);
        });

        _services.ConfigureOpenTelemetryMeterProvider((sp, builder) =>
        {
            foreach (var instrumentation in sp.GetServices<IMetricInstrumentation>())
                instrumentation.AddInstrumentation(sp, builder);
            foreach (var exporter in sp.GetServices<IMetricExporter>())
                exporter.AddExporter(builder);
        });

        _services.ConfigureOpenTelemetryLoggerProvider((sp, builder) =>
        {
            foreach (var exporter in sp.GetServices<ILoggingExporter>())
                exporter.Configure(builder, sp);
        });
    }

    public JarvisOpenTelemetryHostBuilder ConfigureResource(IList<KeyValuePair<string, object>>? attributes = null)
    {
        _builder
            .ConfigureResource(configure =>
            {
                attributes ??= new List<KeyValuePair<string, object>>();

                var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                if (string.IsNullOrEmpty(env))
                    env = "Development";

                attributes.Add(new KeyValuePair<string, object>("deployment.environment", env.ToLowerInvariant()));

                if (_options.Attributes is { Count: > 0 })
                {
                    foreach (var item in _options.Attributes)
                        attributes.Add(new KeyValuePair<string, object>(item.Key, item.Value));
                }

                var entryAssembly = Assembly.GetEntryAssembly();
                var assemblyName = entryAssembly?.GetName();

                configure
                    .AddEnvironmentVariableDetector()
                    .AddTelemetrySdk()
                    .AddAttributes(attributes)
                    .AddService(
                        serviceName: string.IsNullOrEmpty(_options.Name)
                            ? assemblyName?.Name ?? string.Empty
                            : _options.Name,
                        serviceNamespace: string.IsNullOrEmpty(_options.Namespace)
                            ? null
                            : _options.Namespace,
                        serviceVersion: string.IsNullOrEmpty(_options.Version)
                            ? assemblyName?.Version?.ToString()
                            : _options.Version,
                        serviceInstanceId: string.IsNullOrEmpty(_options.InstanceId)
                            ? Environment.MachineName
                            : _options.InstanceId);

                if (_options.Resource.IncludeHostDetector)
                    configure.AddHostDetector();
                if (_options.Resource.IncludeContainerDetector)
                    configure.AddContainerDetector();
            });

        return this;
    }

    public JarvisOpenTelemetryHostBuilder ConfigureTrace(
        Action<TracerProviderBuilder>? instrumentationAction = null,
        Action<TracerProviderBuilder>? exporterAction = null)
    {
        if (_options.Tracing == null)
            return this;

        _builder.WithTracing(configure =>
        {
            configure
                .AddSource(Assembly.GetEntryAssembly()?.GetName()?.Name ?? string.Empty)
                .SetSampler(TraceSamplerFactory.Create(_options.Tracing))
                .SetErrorStatusOnException(true)
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = _options.Tracing.AspNetCoreInstrumentation.RecordException;
                    options.Filter = ctx => AspNetCoreTracePathFilter.ShouldInstrument(ctx, _options.Tracing.AspNetCoreInstrumentation.ExcludedPathPrefixes);

                    options.EnrichWithHttpRequest = async (activity, request) =>
                    {
                        if (request.HttpContext.RequestServices == null)
                            return;

                        var services = request.HttpContext.RequestServices.GetServices<IAspNetCoreEnrichHttpRequest>();
                        foreach (var item in services)
                            await item.EnrichAsync(activity, request);
                    };

                    options.EnrichWithHttpResponse = async (activity, response) =>
                    {
                        if (response.HttpContext.RequestServices == null)
                            return;

                        var services = response.HttpContext.RequestServices.GetServices<IAspNetCoreEnrichHttpResponse>();
                        foreach (var item in services)
                            await item.EnrichAsync(activity, response);
                    };

                    options.EnrichWithException = (activity, exception) =>
                    {
                        activity.SetTag("app.exception.errorCode", exception.HResult);
                    };
                })
                .AddHttpClientInstrumentation(options =>
                {
                    options.RecordException = _options.Tracing.AspNetCoreInstrumentation.RecordException;
                    options.EnrichWithException = (activity, exception) =>
                    {
                        activity.SetTag("app.exception.errorCode", exception.HResult);
                    };

                    if (_options.Tracing.HttpClient.ExcludedUrlSubstrings.Count > 0)
                    {
                        options.FilterHttpRequestMessage = request =>
                        {
                            var uri = request.RequestUri?.ToString() ?? string.Empty;
                            foreach (var sub in _options.Tracing.HttpClient.ExcludedUrlSubstrings)
                            {
                                if (string.IsNullOrEmpty(sub))
                                    continue;
                                if (uri.Contains(sub, StringComparison.OrdinalIgnoreCase))
                                    return false;
                            }

                            return true;
                        };
                    }
                });

            instrumentationAction?.Invoke(configure);

            if (IncludeTracingConsoleExporter())
                configure.AddConsoleExporter();

            if (exporterAction != null)
                exporterAction.Invoke(configure);
            else if (ShouldRegisterTracingOtlp())
            {
                configure.AddOtlpExporter(otlp =>
                {
                    otlp.Endpoint = _options.Tracing.Endpoint;
                    otlp.Headers = _options.Tracing.Headers;
                    otlp.Protocol = _options.Tracing.Protocol;
                    otlp.ExportProcessorType = _options.Tracing.ExportProcessorType;
                    otlp.TimeoutMilliseconds = _options.Tracing.TimeoutMilliseconds;
                    otlp.BatchExportProcessorOptions.ScheduledDelayMilliseconds = _options.Tracing.BatchExportProcessorOptions.ScheduledDelayMilliseconds;
                    otlp.BatchExportProcessorOptions.MaxQueueSize = _options.Tracing.BatchExportProcessorOptions.MaxQueueSize;
                    otlp.BatchExportProcessorOptions.MaxExportBatchSize = _options.Tracing.BatchExportProcessorOptions.MaxExportBatchSize;
                });
            }
        });

        return this;
    }

    public JarvisOpenTelemetryHostBuilder ConfigureMetric(
        Action<MeterProviderBuilder>? metricAction = null,
        Action<MeterProviderBuilder>? exporterAction = null)
    {
        if (_options.Metric == null)
            return this;

        _builder.WithMetrics(configure =>
        {
            configure
                .AddMeter(Assembly.GetEntryAssembly()?.GetName()?.Name ?? string.Empty)
                .AddRuntimeInstrumentation()
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddEventCountersInstrumentation()
                .AddProcessInstrumentation();

            if (string.Equals(_options.Metric.HistogramAggregation, "exponential", StringComparison.OrdinalIgnoreCase))
            {
                configure.AddView(instrument =>
                {
                    return instrument.GetType().GetGenericTypeDefinition() == typeof(Histogram<>)
                        ? new Base2ExponentialBucketHistogramConfiguration()
                        : null;
                });
            }

            metricAction?.Invoke(configure);

            if (IncludeMetricConsoleExporter())
                configure.AddConsoleExporter();

            if (exporterAction != null)
                exporterAction.Invoke(configure);
            else if (ShouldRegisterMetricsOtlp())
            {
                configure.AddOtlpExporter(otlp =>
                {
                    otlp.Endpoint = _options.Metric.Endpoint;
                    otlp.Headers = _options.Metric.Headers;
                    otlp.Protocol = _options.Metric.Protocol;
                    otlp.ExportProcessorType = _options.Metric.ExportProcessorType;
                    otlp.TimeoutMilliseconds = _options.Metric.TimeoutMilliseconds;
                    otlp.BatchExportProcessorOptions.ScheduledDelayMilliseconds = _options.Metric.BatchExportProcessorOptions.ScheduledDelayMilliseconds;
                    otlp.BatchExportProcessorOptions.MaxQueueSize = _options.Metric.BatchExportProcessorOptions.MaxQueueSize;
                    otlp.BatchExportProcessorOptions.MaxExportBatchSize = _options.Metric.BatchExportProcessorOptions.MaxExportBatchSize;
                });
            }
        });

        return this;
    }

    public JarvisOpenTelemetryHostBuilder ConfigureLogging(
        Action<OpenTelemetryLoggerOptions>? logAction = null,
        Action<OpenTelemetryLoggerOptions>? exporterAction = null)
    {
        var logging = _options.Logging;

        _services.AddLogging(builder =>
        {
            builder.Configure(opt =>
            {
                opt.ActivityTrackingOptions = ActivityTrackingOptions.TraceId | ActivityTrackingOptions.SpanId;
            });

            builder.AddOpenTelemetry(configure =>
            {
                configure.IncludeFormattedMessage = logging.IncludeFormattedMessage;
                configure.IncludeScopes = logging.IncludeScopes;
                configure.ParseStateValues = logging.ParseStateValues;

                logAction?.Invoke(configure);

                if (logging.IncludeConsoleExporter)
                    configure.AddConsoleExporter();

                if (exporterAction != null)
                    exporterAction.Invoke(configure);
                else if (ShouldRegisterLoggingOtlp())
                {
                    configure.AddOtlpExporter(otlp =>
                    {
                        otlp.Endpoint = logging.Endpoint;
                        otlp.Headers = logging.Headers;
                        otlp.Protocol = logging.Protocol;
                        otlp.ExportProcessorType = logging.ExportProcessorType;
                        otlp.TimeoutMilliseconds = logging.TimeoutMilliseconds;
                        otlp.BatchExportProcessorOptions.ScheduledDelayMilliseconds = logging.BatchExportProcessorOptions.ScheduledDelayMilliseconds;
                        otlp.BatchExportProcessorOptions.MaxQueueSize = logging.BatchExportProcessorOptions.MaxQueueSize;
                        otlp.BatchExportProcessorOptions.MaxExportBatchSize = logging.BatchExportProcessorOptions.MaxExportBatchSize;
                    });
                }
            });
        });

        return this;
    }

    private bool IncludeTracingConsoleExporter() => _options.Tracing?.IncludeConsoleExporter == true;

    private bool IncludeMetricConsoleExporter() => _options.Metric?.IncludeConsoleExporter == true;

    private bool ShouldRegisterTracingOtlp() =>
        !string.IsNullOrWhiteSpace(_otelSection.GetSection("Tracing")["Endpoint"])
        || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT"))
        || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_TRACES_ENDPOINT"));

    private bool ShouldRegisterMetricsOtlp() =>
        !string.IsNullOrWhiteSpace(_otelSection.GetSection("Metric")["Endpoint"])
        || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT"))
        || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_METRICS_ENDPOINT"));

    private bool ShouldRegisterLoggingOtlp() =>
        !string.IsNullOrWhiteSpace(_otelSection.GetSection("Logging")["Endpoint"])
        || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT"))
        || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_LOGS_ENDPOINT"));
}
