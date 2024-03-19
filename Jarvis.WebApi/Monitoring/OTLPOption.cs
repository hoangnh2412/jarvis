using static Jarvis.WebApi.Monitoring.OTLPOption;

namespace Jarvis.WebApi.Monitoring;

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
        public ExporterType Exporter { get; set; }
        public string Endpoint { get; set; }
    }

    public class MetricOption
    {
        public ExporterType Exporter { get; set; }
        public string Endpoint { get; set; }
    }

    public class TracingOption
    {
        public ExporterType Exporter { get; set; }
        public string Endpoint { get; set; }
    }

    public enum ExporterType
    {
        Console = 1,
        OTLP = 2,
        Prometheus = 3,
        Uptrace = 4
    }

    public enum InstrumentationType
    {
        Redis = 1,
        RabbitMQ = 2,
        Elasticsearch = 3
    }
}

public static class OTLPType
{
    public static Dictionary<ExporterType, string> TraceExporters = new Dictionary<ExporterType, string>();
    public static Dictionary<ExporterType, string> MetricExporters = new Dictionary<ExporterType, string>();
    public static Dictionary<ExporterType, string> LoggingExporters = new Dictionary<ExporterType, string>();

    public static Dictionary<InstrumentationType, string> TraceInstrumentations = new Dictionary<InstrumentationType, string>();
    public static Dictionary<InstrumentationType, string> MetricInstrumentations = new Dictionary<InstrumentationType, string>();
    public static Dictionary<InstrumentationType, string> LoggingInstrumentations = new Dictionary<InstrumentationType, string>();
}