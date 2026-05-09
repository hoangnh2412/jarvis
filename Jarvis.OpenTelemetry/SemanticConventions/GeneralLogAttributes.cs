namespace Jarvis.OpenTelemetry.SemanticConventions;

/// <summary>
/// General log-related attribute names.
/// See https://opentelemetry.io/docs/specs/semconv/general/logs/
/// </summary>
public static class GeneralLogAttributes
{
    public const string RecordUid = "log.record.uid";
    public const string FileName = "log.file.name";
    public const string FileNameResolved = "log.file.name_resolved";
    public const string FilePath = "log.file.path";
    public const string FilePathResolved = "log.file.path_resolved";
}
