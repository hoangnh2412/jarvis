namespace Jarvis.OpenTelemetry;

public class OTELOption
{
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string InstanceId { get; set; } = string.Empty;
    public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
    public LoggingOption Logging { get; set; } = new LoggingOption();
    public MetricOption Metric { get; set; } = new MetricOption();
    public TracingOption Tracing { get; set; } = new TracingOption();
    public string HistogramAggregation { get; set; } = string.Empty;
    public AspNetCoreInstrumentationOption AspNetCoreInstrumentation { get; set; } = new AspNetCoreInstrumentationOption();

    public class AspNetCoreInstrumentationOption
    {
        public bool RecordException { get; set; }
    }

    public class LoggingOption
    {
        public bool IncludeConsoleExporter { get; set; }
        public string Endpoint { get; set; } = string.Empty;
        public string Headers { get; set; } = string.Empty;
    }

    public class MetricOption
    {
        public bool IncludeConsoleExporter { get; set; }
        public string Endpoint { get; set; } = string.Empty;
        public string Headers { get; set; } = string.Empty;
    }

    public class TracingOption
    {
        public bool IncludeConsoleExporter { get; set; }
        public string Endpoint { get; set; } = string.Empty;
        public string Headers { get; set; } = string.Empty;
    }
}