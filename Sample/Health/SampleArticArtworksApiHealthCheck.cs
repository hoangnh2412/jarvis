using System.Diagnostics;
using System.Net.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Sample.Health;

/// <summary>Readiness probe: GET <see href="https://api.artic.edu/api/v1/artworks">Art Institute artworks</see> (limit=1 for small payload).</summary>
public sealed class SampleArticArtworksApiHealthCheck(IHttpClientFactory httpClientFactory) : IHealthCheck
{
    public const string HttpClientName = "SampleArticArtworksApi";
    private static readonly Uri ArtworksUrl = new("https://api.artic.edu/api/v1/artworks?limit=1");

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var client = httpClientFactory.CreateClient(HttpClientName);
            using var response = await client
                .GetAsync(ArtworksUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);
            sw.Stop();
            var code = (int)response.StatusCode;
            var detail = $"http={code} elapsed_ms={sw.ElapsedMilliseconds}";
            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy(detail)
                : HealthCheckResult.Unhealthy($"artic_artworks_api fail {detail}");
        }
        catch (Exception ex)
        {
            sw.Stop();
            return HealthCheckResult.Unhealthy($"artic_artworks_api error elapsed_ms={sw.ElapsedMilliseconds}", ex);
        }
    }
}
