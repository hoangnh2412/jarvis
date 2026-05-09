namespace Jarvis.OpenTelemetry.Configuration;

/// <summary>
/// Controls which HTTP headers are copied onto spans (allowlist only; avoids high cardinality and sensitive data by default).
/// </summary>
public class HttpTraceEnrichmentOptions
{
    /// <summary>When true, allowed request headers are added as <c>http.request.header.*</c> attributes.</summary>
    public bool CaptureRequestHeaders { get; set; }

    /// <summary>When true, allowed response headers are added as <c>http.response.header.*</c> attributes.</summary>
    public bool CaptureResponseHeaders { get; set; }

    /// <summary>Header names allowed for the request (case-insensitive). Ignored when <see cref="CaptureRequestHeaders"/> is false.</summary>
    public IList<string> AllowedRequestHeaderNames { get; set; } = new List<string>();

    /// <summary>Header names allowed for the response (case-insensitive).</summary>
    public IList<string> AllowedResponseHeaderNames { get; set; } = new List<string>();

    /// <summary>Truncates each header value to this length (after ToString).</summary>
    public int MaxHeaderValueLength { get; set; } = 256;
}
