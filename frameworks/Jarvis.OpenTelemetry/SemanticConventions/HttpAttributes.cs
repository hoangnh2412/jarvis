namespace Jarvis.OpenTelemetry.SemanticConventions;

/// <summary>
/// Semantic convention attributes in the HTTP namespace.
/// See https://opentelemetry.io/docs/specs/semconv/registry/http/
/// </summary>
public static class HttpAttributes
{
    /// <summary>
    /// HTTP request headers; <c>{0}</c> is the normalized header name (lowercase).
    /// </summary>
    public const string RequestHeader = "http.request.header.{0}";

    public const string RequestSize = "http.request.size";

    public const string ResponseHeader = "http.response.header.{0}";

    public const string ResponseSize = "http.response.size";
}
