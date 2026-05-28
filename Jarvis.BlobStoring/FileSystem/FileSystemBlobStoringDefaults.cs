namespace Jarvis.BlobStoring.FileSystem;

internal static class FileSystemBlobStoringDefaults
{
    public const int DefaultAutoSelectPriority = 10;

    public static int ResolveAutoSelectPriority(FileSystemOption options) => options.AutoSelectPriority > 0 ? options.AutoSelectPriority : DefaultAutoSelectPriority;
}
