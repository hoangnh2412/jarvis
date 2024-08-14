namespace Jarvis.Shared.Options;

public class OTLPOption
{
    public string ServiceInstanceId { get; set; }
    public string ServiceName { get; set; }
    public string ServiceVersion { get; set; }
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
        public ExporterType? Exporter { get; set; }
        public string Endpoint { get; set; }
    }

    public class MetricOption
    {
        public ExporterType? Exporter { get; set; }
        public string Endpoint { get; set; }
    }

    public class TracingOption
    {
        public ExporterType? Exporter { get; set; }
        public string Endpoint { get; set; }
    }

    public enum ExporterType
    {
        Console = 1,
        OTLP = 2,
        Prometheus = 3
    }
}