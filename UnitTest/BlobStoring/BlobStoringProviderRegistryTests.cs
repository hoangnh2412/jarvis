using Jarvis.BlobStoring;
using Jarvis.BlobStoring.Hosting;

namespace UnitTest.BlobStoring;

public class BlobStoringProviderRegistryTests
{
    [Fact]
    public void ResolveDefaultProviderKey_Uses_Explicit_DefaultProvider()
    {
        var registry = new BlobStoringProviderRegistry();
        registry.Register(nameof(BlobStoringType.MinIO), 30);
        registry.Register(nameof(BlobStoringType.FileSystem), 10);

        Assert.Equal(
            nameof(BlobStoringType.FileSystem),
            registry.ResolveDefaultProviderKey(nameof(BlobStoringType.FileSystem)));
    }

    [Fact]
    public void ResolveDefaultProviderKey_Picks_Highest_Priority_Registered_Provider()
    {
        var registry = new BlobStoringProviderRegistry();
        registry.Register(nameof(BlobStoringType.FileSystem), 10);
        registry.Register(nameof(BlobStoringType.AwsS3), 20);
        registry.Register(nameof(BlobStoringType.MinIO), 30);

        Assert.Equal(nameof(BlobStoringType.MinIO), registry.ResolveDefaultProviderKey(null));
        Assert.Equal(nameof(BlobStoringType.MinIO), registry.ResolveDefaultProviderKey(""));

        registry = new BlobStoringProviderRegistry();
        registry.Register(nameof(BlobStoringType.FileSystem), 10);
        registry.Register(nameof(BlobStoringType.AwsS3), 20);

        Assert.Equal(nameof(BlobStoringType.AwsS3), registry.ResolveDefaultProviderKey(null));
    }

    [Fact]
    public void ResolveDefaultProviderKey_Falls_Back_To_FileSystem_When_Empty()
    {
        var registry = new BlobStoringProviderRegistry();

        Assert.Equal(nameof(BlobStoringType.FileSystem), registry.ResolveDefaultProviderKey(null));
    }
}
