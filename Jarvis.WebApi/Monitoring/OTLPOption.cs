namespace Jarvis.WebApi.Monitoring;

public class OTLPOption
{
    public string Name { get; set; }
    public string Namespace { get; set; }
    public string Version { get; set; }
    public string InstanceId { get; set; }
    public Dictionary<string, string> Attributes { get; set; }
    public LoggingOption Logging { get; set; }
    public MetricOption Metric { get; set; }
    public TracingOption Tracing { get; set; }
    public string HistogramAggregation { get; set; }
    public AspNetCoreInstrumentationOption AspNetCoreInstrumentation { get; set; }

    public class AspNetCoreInstrumentationOption
    {
        public bool RecordException { get; set; }
    }

    public class LoggingOption
    {
        public bool IncludeConsoleExporter { get; set; }
        public string Endpoint { get; set; }
        public string Headers { get; set; }
    }

    public class MetricOption
    {
        public bool IncludeConsoleExporter { get; set; }
        public string Endpoint { get; set; }
        public string Headers { get; set; }
    }

    public class TracingOption
    {
        public bool IncludeConsoleExporter { get; set; }
        public string Endpoint { get; set; }
        public string Headers { get; set; }
    }
}