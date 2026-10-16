using Platform.BlobStoring;

namespace Platform.Modules.Files.Storage;

/// <summary>Resolves the active <see cref="IBlobStoringService"/> for file operations.</summary>
public interface IFileStorageResolver
{
    IBlobStoringService Resolve();
}
