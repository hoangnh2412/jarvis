using System.Diagnostics;
using Jarvis.OpenTelemetry.Abstractions;
using Jarvis.OpenTelemetry.Configuration;
using Jarvis.OpenTelemetry.SemanticConventions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Jarvis.OpenTelemetry.Instrumentations;

/// <summary>
/// Adds allowlisted response headers to the current activity. Disabled by default.
/// </summary>
public sealed class HttpResponseHeaderEnrichment(IOptions<JarvisOpenTelemetryOptions> options)
    : IAspNetCoreEnrichHttpResponse
{
    private readonly JarvisOpenTelemetryOptions _options = options.Value;

    public Task EnrichAsync(Activity activity, HttpResponse httpResponse)
    {
        var enrichment = _options.Tracing.HttpTraceEnrichment;
        if (!enrichment.CaptureResponseHeaders)
            return Task.CompletedTask;

        var allowed = HttpHeaderEnrichmentHelper.ToAllowedSet(enrichment.AllowedResponseHeaderNames);
        if (allowed.Count == 0)
            return Task.CompletedTask;

        var maxLen = enrichment.MaxHeaderValueLength;

        foreach (var header in httpResponse.Headers)
        {
            if (!allowed.Contains(header.Key))
                continue;

            var normalized = HttpHeaderEnrichmentHelper.NormalizeHeaderNameForAttribute(header.Key);
            var value = HttpHeaderEnrichmentHelper.Truncate(header.Value.ToString(), maxLen);
            activity.SetTag(string.Format(HttpAttributes.ResponseHeader, normalized), value);
        }

        return Task.CompletedTask;
    }
}
