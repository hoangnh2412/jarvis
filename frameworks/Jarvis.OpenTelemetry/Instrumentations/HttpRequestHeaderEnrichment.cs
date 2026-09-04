using System.Diagnostics;
using Jarvis.OpenTelemetry.Abstractions;
using Jarvis.OpenTelemetry.Configuration;
using Jarvis.OpenTelemetry.SemanticConventions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Jarvis.OpenTelemetry.Instrumentations;

/// <summary>
/// Adds allowlisted request headers to the current activity. Disabled by default; enable via <see cref="TraceSignalOptions.HttpTraceEnrichment"/>.
/// </summary>
public sealed class HttpRequestHeaderEnrichment(IOptions<JarvisOpenTelemetryOptions> options)
    : IAspNetCoreEnrichHttpRequest
{
    private readonly JarvisOpenTelemetryOptions _options = options.Value;

    public Task EnrichAsync(Activity activity, HttpRequest httpRequest)
    {
        var enrichment = _options.Tracing.HttpTraceEnrichment;
        if (!enrichment.CaptureRequestHeaders)
            return Task.CompletedTask;

        var allowed = HttpHeaderEnrichmentHelper.ToAllowedSet(enrichment.AllowedRequestHeaderNames);
        if (allowed.Count == 0)
            return Task.CompletedTask;

        var maxLen = enrichment.MaxHeaderValueLength;

        foreach (var header in httpRequest.Headers)
        {
            if (!allowed.Contains(header.Key))
                continue;

            var normalized = HttpHeaderEnrichmentHelper.NormalizeHeaderNameForAttribute(header.Key);
            var value = HttpHeaderEnrichmentHelper.Truncate(header.Value.ToString(), maxLen);
            activity.SetTag(string.Format(HttpAttributes.RequestHeader, normalized), value);
        }

        return Task.CompletedTask;
    }
}
