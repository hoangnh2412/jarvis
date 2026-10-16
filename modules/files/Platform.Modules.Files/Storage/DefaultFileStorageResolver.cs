using Platform.BlobStoring;

namespace Platform.Modules.Files.Storage;

/// <summary>Uses the default registered <see cref="IBlobStoringService"/> from blob storing DI.</summary>
public sealed class DefaultFileStorageResolver(IBlobStoringService blobStoring) : IFileStorageResolver
{
    public IBlobStoringService Resolve() => blobStoring;
}
