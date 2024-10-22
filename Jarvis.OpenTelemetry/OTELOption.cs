using OpenTelemetry;
using OpenTelemetry.Exporter;

namespace Jarvis.OpenTelemetry;

public class OTELOption
{
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string InstanceId { get; set; } = string.Empty;
    public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
    public ExporterOption Logging { get; set; } = new ExporterOption();
    public ExporterOption Metric { get; set; } = new ExporterOption();
    public ExporterOption Tracing { get; set; } = new ExporterOption();
    public string HistogramAggregation { get; set; } = string.Empty;
    public AspNetCoreInstrumentationOption AspNetCoreInstrumentation { get; set; } = new AspNetCoreInstrumentationOption();

    public class AspNetCoreInstrumentationOption
    {
        public bool RecordException { get; set; }
    }

    public class ExporterOption
    {
        public string Endpoint { get; set; } = string.Empty;
        public string Headers { get; set; } = string.Empty;
        public bool IncludeConsoleExporter { get; set; } = false;
        public OtlpExportProtocol Protocol { get; set; } = OtlpExportProtocol.Grpc;
        public ExportProcessorType ExportProcessorType { get; set; } = ExportProcessorType.Batch;
    }
}