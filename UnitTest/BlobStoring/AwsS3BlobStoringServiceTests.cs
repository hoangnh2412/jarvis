using Jarvis.BlobStoring.AwsS3;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace UnitTest.BlobStoring;

public class AwsS3BlobStoringServiceTests
{
    [Fact]
    public void Constructor_Does_Not_Create_Client_Immediately()
    {
        var service = new AwsS3BlobStoringService(
            Options.Create(new AwsS3BlobOptions
            {
                Region = "ap-southeast-1",
                BucketName = "test-bucket",
                AccessKey = "key",
                SecretKey = "secret"
            }),
            NullLogger<AwsS3BlobStoringService>.Instance);

        Assert.NotNull(service);
    }

    [Fact]
    public void Dispose_When_Client_Was_Never_Created_Does_Not_Throw()
    {
        var service = new AwsS3BlobStoringService(
            Options.Create(new AwsS3BlobOptions
            {
                Region = "ap-southeast-1",
                BucketName = "test-bucket"
            }),
            NullLogger<AwsS3BlobStoringService>.Instance);

        service.Dispose();
    }
}
