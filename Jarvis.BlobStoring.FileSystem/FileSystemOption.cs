namespace Jarvis.BlobStoring.FileSystem;

public class FileSystemOption
{
    public string RootPath { get; set; } = string.Empty;
    public string SubPath { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public int PartitionBy { get; set; }
}