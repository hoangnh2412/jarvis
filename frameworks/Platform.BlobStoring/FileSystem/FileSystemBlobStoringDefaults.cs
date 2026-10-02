using Platform.BlobStoring.Configuration;

namespace Platform.BlobStoring.FileSystem;

internal static class FileSystemBlobStoringDefaults
{
    public const int DefaultAutoSelectPriority = 10;

    public static int ResolveAutoSelectPriority(FileSystemBlobOptions options) =>
        options.AutoSelectPriority > 0 ? options.AutoSelectPriority : DefaultAutoSelectPriority;
}
