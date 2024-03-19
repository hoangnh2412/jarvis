using Microsoft.Extensions.Options;
using OpenTelemetry.Trace;
using Uptrace.OpenTelemetry;

namespace Jarvis.WebApi.Monitoring;

public class UptraceTraceExporter : ITraceExporter
{
    private readonly OTLPOption _options;

    public UptraceTraceExporter(
        OTLPOption options)
    {
        _options = options;
    }

    public TracerProviderBuilder AddExporter(TracerProviderBuilder builder)
    {
        builder.AddUptrace(_options.Tracing.Endpoint);
        return builder;
    }
}