using System.Diagnostics.Metrics;
using System.Reflection;
using Jarvis.WebApi.Monitoring.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Jarvis.WebApi.Monitoring;

public class OTLPBuilder
{
    private readonly IServiceCollection _services;
    private readonly OTLPOption _otlpOption;
    private readonly OpenTelemetryBuilder _otelBuilder;

    public OTLPBuilder(
        IServiceCollection services,
        OTLPOption otlpOption)
    {
        _services = services;
        _otlpOption = otlpOption;
        _otelBuilder = _services.AddOpenTelemetry();
    }

    public OTLPBuilder ConfigureResource(IList<KeyValuePair<string, object>> attributes = null)
    {
        _otelBuilder
            .ConfigureResource(configure =>
            {
                if (attributes == null)
                    attributes = new List<KeyValuePair<string, object>>();

                var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                if (string.IsNullOrEmpty(env))
                    env = "Development";

                attributes.Add(new KeyValuePair<string, object>("deployment.environment", env));

                if (_otlpOption.Attributes != null && _otlpOption.Attributes.Count > 0)
                {
                    foreach (var item in _otlpOption.Attributes)
                    {
                        attributes.Add(new KeyValuePair<string, object>(item.Key, item.Value));
                    }
                }

                configure
                    .AddEnvironmentVariableDetector()
                    .AddTelemetrySdk()
                    .AddAttributes(attributes)
                    .AddService(
                        serviceName: string.IsNullOrEmpty(_otlpOption.Name) ? Assembly.GetEntryAssembly().GetName().Name : _otlpOption.Name,
                        serviceNamespace: string.IsNullOrEmpty(_otlpOption.Namespace) ? Assembly.GetEntryAssembly().GetName().Name : _otlpOption.Namespace,
                        serviceVersion: string.IsNullOrEmpty(_otlpOption.Version) ? Assembly.GetEntryAssembly().GetName().Version?.ToString() : _otlpOption.Version,
                        serviceInstanceId: string.IsNullOrEmpty(_otlpOption.InstanceId) ? Environment.MachineName : _otlpOption.InstanceId);
            });

        return this;
    }

    public OTLPBuilder ConfigureTrace(Action<TracerProviderBuilder> instrumentationAction = null, Action<TracerProviderBuilder> exporterAction = null)
    {
        if (_otlpOption.Tracing == null)
            return this;

        _otelBuilder.WithTracing(configure =>
        {
            configure
                .AddSource(Assembly.GetEntryAssembly().GetName().Name)
                .SetSampler(new AlwaysOnSampler())
                .SetErrorStatusOnException(true)
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = true;

                    options.EnrichWithHttpRequest = (activity, request) =>
                    {
                        if (request.HttpContext.RequestServices == null)
                            return;

                        var services = request.HttpContext.RequestServices.GetServices<IAspNetCoreHttpRequest>();
                        foreach (var item in services)
                        {
                            item.Enrich(activity, request);
                        }
                    };

                    options.EnrichWithHttpResponse = (activity, response) =>
                    {
                        if (response.HttpContext.RequestServices == null)
                            return;

                        var services = response.HttpContext.RequestServices.GetServices<IAspNetCoreEntricHttpResponse>();
                        foreach (var item in services)
                        {
                            item.Enrich(activity, response);
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

            if (instrumentationAction != null)
                instrumentationAction.Invoke(configure);

            if (_otlpOption.Tracing.IncludeConsoleExporter)
                configure.AddConsoleExporter();

            if (exporterAction != null)
            {
                exporterAction.Invoke(configure);
            }
            else
            {
                configure.AddOtlpExporter(otp =>
                {
                    otp.Endpoint = new Uri(_otlpOption.Tracing.Endpoint);
                    otp.Headers = _otlpOption.Tracing.Headers;
                });
            }
        });

        return this;
    }

    public OTLPBuilder ConfigureMetric(Action<MeterProviderBuilder> metricAction = null, Action<MeterProviderBuilder> exporterAction = null)
    {
        if (_otlpOption.Metric == null)
            return this;

        _otelBuilder.WithMetrics(configure =>
        {
            configure
                .AddMeter(Assembly.GetEntryAssembly().GetName().Name)
                .AddRuntimeInstrumentation()
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation();

            switch (_otlpOption.HistogramAggregation)
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

            if (metricAction != null)
                metricAction.Invoke(configure);

            if (_otlpOption.Metric.IncludeConsoleExporter)
                configure.AddConsoleExporter();

            if (exporterAction != null)
            {
                exporterAction.Invoke(configure);
            }
            else
            {
                configure.AddOtlpExporter(otp =>
                {
                    otp.Endpoint = new Uri(_otlpOption.Metric.Endpoint);
                    otp.Headers = _otlpOption.Metric.Headers;
                });
            }
        });

        // _otelBuilder.Services.ConfigureOpenTelemetryMeterProvider((serviceProvider, configure) =>
        // {
        //     var instrumentations = serviceProvider.GetServices<IMetricInstrumentation>();
        //     foreach (var item in instrumentations)
        //     {
        //         item.Configure(serviceProvider, configure);
        //     }

        //     if (otlpOption.Value.Metric.IncludeConsoleExporter)
        //         configure.AddConsoleExporter();

        //     var exporters = serviceProvider.GetServices<IMetricExporter>();
        //     if (exporters.Count() == 0)
        //     {
        //         configure.AddOtlpExporter(otp =>
        //         {
        //             otp.Endpoint = new Uri(otlpOption.Value.Metric.Endpoint);
        //             otp.Headers = otlpOption.Value.Metric.Headers;
        //         });
        //     }
        //     else
        //     {
        //         foreach (var item in exporters)
        //         {
        //             item.Confiture(configure);
        //         }
        //     }
        // });

        return this;
    }

    public OTLPBuilder ConfigureLogging(Action<OpenTelemetryLoggerOptions> logAction = null, Action<OpenTelemetryLoggerOptions> exporterAction = null)
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

                if (logAction != null)
                    logAction.Invoke(configure);

                if (_otlpOption.Logging.IncludeConsoleExporter)
                    configure.AddConsoleExporter();

                if (exporterAction != null)
                {
                    exporterAction.Invoke(configure);
                }
                else
                {
                    configure.AddOtlpExporter(otp =>
                    {
                        otp.Endpoint = new Uri(_otlpOption.Logging.Endpoint);
                        otp.Headers = _otlpOption.Logging.Headers;
                    });
                }
            });
        });

        return this;
    }
}