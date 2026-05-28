namespace Jarvis.BlobStoring.FileSystem;

public class FileSystemOption
{
    public string RootPath { get; set; } = string.Empty;

    public string SubPath { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string BucketName { get; set; } = string.Empty;

    public int PartitionBy { get; set; }

    /// <summary>Auto-default priority when <c>DefaultProvider</c> is empty. 0 = use built-in default (10).</summary>
    public int AutoSelectPriority { get; set; }
}
