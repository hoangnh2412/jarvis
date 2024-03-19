namespace Jarvis.Infrastructure.BlobStoring.MinIO;

/// <summary>
/// HttpClient support MinIO SDK old version
/// </summary>
public interface IMinioHttpClient
{
    HttpClient GetClient();
}

public class MinioHttpClient : IMinioHttpClient
{
    private readonly HttpClient _client;
    public MinioHttpClient()
    {
        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, error) => { return true; };
        _client = new HttpClient(handler);
    }

    public HttpClient GetClient()
    {
        return _client;
    }
}