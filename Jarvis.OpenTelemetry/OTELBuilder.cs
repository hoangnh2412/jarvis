using System.Diagnostics.Metrics;
using System.Reflection;
using Jarvis.OpenTelemetry.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Jarvis.OpenTelemetry;

public class OTELBuilder
{
    private readonly IServiceCollection _services;
    private readonly OTELOption _option;
    private readonly OpenTelemetryBuilder _builder;

    public OTELBuilder(
        IServiceCollection services,
        OTELOption option)
    {
        _services = services;
        _option = option;
        _builder = _services.AddOpenTelemetry();
    }

    public OTELBuilder ConfigureResource(IList<KeyValuePair<string, object>>? attributes = null)
    {
        _builder
            .ConfigureResource(configure =>
            {
                if (attributes == null)
                    attributes = new List<KeyValuePair<string, object>>();

                var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                if (string.IsNullOrEmpty(env))
                    env = "Development";

                attributes.Add(new KeyValuePair<string, object>("deployment.environment", env));

                if (_option.Attributes != null && _option.Attributes.Count > 0)
                {
                    foreach (var item in _option.Attributes)
                    {
                        attributes.Add(new KeyValuePair<string, object>(item.Key, item.Value));
                    }
                }

                configure
                    .AddEnvironmentVariableDetector()
                    .AddTelemetrySdk()
                    .AddAttributes(attributes)
                    .AddService(
                        serviceName: string.IsNullOrEmpty(_option.Name) ? Assembly.GetEntryAssembly()?.GetName()?.Name ?? string.Empty : _option.Name,
                        serviceNamespace: string.IsNullOrEmpty(_option.Namespace) ? Assembly.GetEntryAssembly()?.GetName().Name : _option.Namespace,
                        serviceVersion: string.IsNullOrEmpty(_option.Version) ? Assembly.GetEntryAssembly()?.GetName().Version?.ToString() : _option.Version,
                        serviceInstanceId: string.IsNullOrEmpty(_option.InstanceId) ? Environment.MachineName : _option.InstanceId);
            });

        return this;
    }

    public OTELBuilder ConfigureTrace(Action<TracerProviderBuilder>? instrumentationAction = null, Action<TracerProviderBuilder>? exporterAction = null)
    {
        if (_option.Tracing == null)
            return this;

        _builder.WithTracing(configure =>
        {
            configure
                .AddSource(Assembly.GetEntryAssembly()?.GetName()?.Name ?? string.Empty)
                .SetSampler(new AlwaysOnSampler())
                .SetErrorStatusOnException(true)
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = true;

                    options.EnrichWithHttpRequest = async (activity, request) =>
                    {
                        if (request.HttpContext.RequestServices == null)
                            return;

                        var services = request.HttpContext.RequestServices.GetServices<IAspNetCoreEnrichHttpRequest>();
                        foreach (var item in services)
                        {
                            await item.EnrichAsync(activity, request);
                        }
                    };

                    options.EnrichWithHttpResponse = async (activity, response) =>
                    {
                        if (response.HttpContext.RequestServices == null)
                            return;

                        var services = response.HttpContext.RequestServices.GetServices<IAspNetCoreEnrichHttpResponse>();
                        foreach (var item in services)
                        {
                            await item.EnrichAsync(activity, response);
                        }
                    };

                    options.EnrichWithException = (activity, exception) =>
                    {
                        activity.SetTag("app.exception.errorCode", exception.HResult);
                    };
                })
                .AddHttpClientInstrumentation(options =>
                {
                    options.RecordException = true;
                    options.EnrichWithException = (activity, exception) =>
                    {
                        activity.SetTag("app.exception.errorCode", exception.HResult);
                    };
                })
                .AddEntityFrameworkCoreInstrumentation(options =>
                {
                    options.SetDbStatementForText = true;
                })
                .AddSqlClientInstrumentation(options =>
                {
                    options.RecordException = true;
                    options.SetDbStatementForText = true;
                });

            instrumentationAction?.Invoke(configure);

            if (_option.Tracing.IncludeConsoleExporter)
                configure.AddConsoleExporter();

            if (exporterAction != null)
            {
                exporterAction.Invoke(configure);
            }
            else
            {
                configure.AddOtlpExporter(otp =>
                {
                    otp.Endpoint = new Uri(_option.Tracing.Endpoint);
                    otp.Headers = _option.Tracing.Headers;
                });
            }
        });

        return this;
    }

    public OTELBuilder ConfigureMetric(Action<MeterProviderBuilder>? metricAction = null, Action<MeterProviderBuilder>? exporterAction = null)
    {
        if (_option.Metric == null)
            return this;

        _builder.WithMetrics(configure =>
        {
            configure
                .AddMeter(Assembly.GetEntryAssembly()?.GetName()?.Name ?? string.Empty)
                .AddRuntimeInstrumentation()
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation();

            switch (_option.HistogramAggregation)
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

            metricAction?.Invoke(configure);

            if (_option.Metric.IncludeConsoleExporter)
                configure.AddConsoleExporter();

            if (exporterAction != null)
            {
                exporterAction.Invoke(configure);
            }
            else
            {
                configure.AddOtlpExporter(otp =>
                {
                    otp.Endpoint = new Uri(_option.Metric.Endpoint);
                    otp.Headers = _option.Metric.Headers;
                });
            }
        });

        return this;
    }

    public OTELBuilder ConfigureLogging(Action<OpenTelemetryLoggerOptions>? logAction = null, Action<OpenTelemetryLoggerOptions>? exporterAction = null)
    {
        _services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.Configure(opt =>
            {
                opt.ActivityTrackingOptions = ActivityTrackingOptions.TraceId | ActivityTrackingOptions.SpanId;
            });

            builder.AddOpenTelemetry(configure =>
            {
                configure.IncludeFormattedMessage = true;
                configure.IncludeScopes = true;
                configure.ParseStateValues = true;

                logAction?.Invoke(configure);

                if (_option.Logging == null || _option.Logging.IncludeConsoleExporter)
                    configure.AddConsoleExporter();

                if (exporterAction != null)
                {
                    exporterAction.Invoke(configure);
                }
                else
                {
                    if (_option.Logging != null)
                    {
                        configure.AddOtlpExporter(otp =>
                        {
                            otp.Endpoint = new Uri(_option.Logging.Endpoint);
                            otp.Headers = _option.Logging.Headers;
                        });
                    }
                }
            });
        });

        return this;
    }
}