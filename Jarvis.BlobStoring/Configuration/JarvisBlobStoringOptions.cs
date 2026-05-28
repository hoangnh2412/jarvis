// Jarvis.BlobStoring — Root options bound from the BlobStoring configuration section.
namespace Jarvis.BlobStoring.Configuration;

/// <summary>
/// Core blob storing options under <c>BlobStoring</c>.
/// Provider-specific sections (MinIO, AwsS3, …) live in their satellite packages.
/// </summary>
public class JarvisBlobStoringOptions
{
    public const string SectionName = "BlobStoring";

    /// <summary>
    /// Keyed provider name for default <see cref="IBlobStoringService"/> resolve
    /// (e.g. FileSystem, MinIO, AwsS3). Empty = highest-priority registered provider.
    /// </summary>
    public string DefaultProvider { get; set; } = string.Empty;
}
