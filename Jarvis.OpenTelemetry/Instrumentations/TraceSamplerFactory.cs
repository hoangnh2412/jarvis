using Jarvis.OpenTelemetry.Configuration;
using OpenTelemetry.Trace;

namespace Jarvis.OpenTelemetry.Instrumentations;

internal static class TraceSamplerFactory
{
    public static Sampler Create(TraceSignalOptions tracing)
    {
        var ratio = tracing.TraceIdRatio;
        if (double.IsNaN(ratio) || ratio < 0)
            ratio = 0;
        else if (ratio > 1)
            ratio = 1;

        var name = tracing.Sampler?.Trim();
        if (string.IsNullOrEmpty(name))
            name = "ParentBasedRatio";

        if (string.Equals(name, "AlwaysOn", StringComparison.OrdinalIgnoreCase))
            return new AlwaysOnSampler();

        if (string.Equals(name, "AlwaysOff", StringComparison.OrdinalIgnoreCase))
            return new AlwaysOffSampler();

        if (string.Equals(name, "TraceIdRatio", StringComparison.OrdinalIgnoreCase))
            return new TraceIdRatioBasedSampler(ratio);

        return new ParentBasedSampler(new TraceIdRatioBasedSampler(ratio));
    }
}
