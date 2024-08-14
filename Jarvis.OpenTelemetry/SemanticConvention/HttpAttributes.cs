namespace Jarvis.OpenTelemetry.SemanticConvention;

/// <summary>
/// Semantic convention attributes in the HTTP namespace. <br />
/// https://opentelemetry.io/docs/specs/semconv/attributes-registry/http/#http-attributes
/// </summary>
public class HttpAttributes
{
    /// <summary>
    /// HTTP request headers, {0} being the normalized HTTP Header name (lowercase), the value being the header values. [1] <br />
    /// Example: http.request.header.content-type=["application/json"]; http.request.header.x-forwarded-for=["1.2.3.4", "1.2.3.5"]
    /// </summary>
    public const string RequestHeader = "http.request.header.{0}";

    /// <summary>
    /// The total size of the request in bytes. This should be the total number of bytes sent over the wire, including the request line (HTTP/1.1), framing (HTTP/2 and HTTP/3), headers, and request body if any.
    /// </summary>
    public const string RequestSize = "http.request.size";

    /// <summary>
    /// HTTP response headers, <key> being the normalized HTTP Header name (lowercase), the value being the header values. [4]
    /// </summary>
    public const string ResponseHeader = "http.response.header.{0}";

    /// <summary>
    /// The total size of the response in bytes. This should be the total number of bytes sent over the wire, including the status line (HTTP/1.1), framing (HTTP/2 and HTTP/3), headers, and response body and trailers if any.
    /// </summary>
    public const string ResponseSize = "http.response.size";
}