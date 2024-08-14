using OpenTelemetry.Trace;
using Jarvis.OpenTelemetry.Interfaces;

namespace Jarvis.OpenTelemetry.Exporters;

public class OTELTraceExporter : ITraceExporter
{
    private readonly OTELOption _options;

    public OTELTraceExporter(
        OTELOption options)
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