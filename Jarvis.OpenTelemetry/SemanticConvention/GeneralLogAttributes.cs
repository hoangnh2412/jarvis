namespace Jarvis.OpenTelemetry.SemanticConvention;

/// <summary>
/// Semantic convention attributes in the generic log. <br />
/// https://opentelemetry.io/docs/specs/semconv/general/logs/
/// </summary>
public class GeneralLogAttributes
{
    /// <summary>
    /// A unique identifier for the Log Record. [1] <br />
    /// Ex: 1234567890
    /// </summary>
    public const string RecordUid = "log.record.uid";

    /// <summary>
    /// The basename of the file. <br />
    /// Ex: audit.log
    /// </summary>
    public const string FileName = "log.file.name";

    /// <summary>
    /// The basename of the file, with symlinks resolved. <br />
    /// Ex: uuid.log
    /// </summary>
    public const string FileNameResolved = "log.file.name_resolved";

    /// <summary>
    /// The full path to the file. <br />
    /// Ex: /var/log/audit.log
    /// </summary>
    public const string FilePath = "log.file.path";

    /// <summary>
    /// The full path to the file, with symlinks resolved. <br />
    /// Ex: /var/log/uuid.log
    /// </summary>
    public const string FilePathResolved = "log.file.path_resolved";
}