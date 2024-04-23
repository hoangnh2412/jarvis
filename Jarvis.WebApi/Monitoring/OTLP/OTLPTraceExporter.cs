using Jarvis.WebApi.Monitoring.Interfaces;
using OpenTelemetry.Trace;

namespace Jarvis.WebApi.Monitoring.Uptrace;

public class OTLPTraceExporter : ITraceExporter
{
    private readonly OTLPOption _options;

    public OTLPTraceExporter(
        OTLPOption options)
    {
        _options = options;
    }

    public TracerProviderBuilder AddExporter(TracerProviderBuilder builder)
    {
        builder.AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(_options.Tracing.Endpoint);
        });
        return builder;
    }
}