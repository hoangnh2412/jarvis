namespace Jarvis.OpenTelemetry.Configuration;

public class HttpClientTraceInstrumentationOptions
{
    /// <summary>
    /// If the request URL contains any of these substrings (ordinal-ignore-case), the outbound call is not traced.
    /// </summary>
    public IList<string> ExcludedUrlSubstrings { get; set; } = new List<string>();
}
