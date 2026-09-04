namespace Jarvis.OpenTelemetry.Configuration;

public class AspNetCoreTraceInstrumentationOptions
{
    public bool RecordException { get; set; } = true;

    /// <summary>
    /// Request path prefixes for which incoming HTTP spans are not created (e.g. <c>/health</c>, <c>/swagger</c>). Comparison is ordinal-ignore-case; values are normalized to start with <c>/</c>.
    /// </summary>
    public IList<string> ExcludedPathPrefixes { get; set; } = new List<string>();
}
