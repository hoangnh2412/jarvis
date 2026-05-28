using Jarvis.BlobStoring.MinIO;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace UnitTest.BlobStoring;

public class MinioBlobStoringServiceSslTests
{
    [Fact]
    public void Constructor_With_UseSsl_Does_Not_Throw()
    {
        var service = new MinioBlobStoringService(
            Options.Create(new MinIOBlobStoringOption
            {
                Endpoint = "localhost:9000",
                AccessKey = "minio",
                SecretKey = "minio123",
                UseSsl = true
            }),
            NullLogger<MinioBlobStoringService>.Instance);

        Assert.NotNull(service);
    }

    [Fact]
    public void Dispose_When_Client_Was_Never_Created_Does_Not_Throw()
    {
        var service = new MinioBlobStoringService(
            Options.Create(new MinIOBlobStoringOption { Endpoint = "localhost:9000" }),
            NullLogger<MinioBlobStoringService>.Instance);

        service.Dispose();
    }
}
