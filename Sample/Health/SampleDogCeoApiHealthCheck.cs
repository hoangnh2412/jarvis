using System.Diagnostics;
using System.Net.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Sample.Health;

/// <summary>Readiness probe: GET <see href="https://dog.ceo/api/breeds/image/random">dog.ceo random breed image</see>.</summary>
public sealed class SampleDogCeoApiHealthCheck(IHttpClientFactory httpClientFactory) : IHealthCheck
{
    public const string HttpClientName = "SampleDogCeoApi";
    private static readonly Uri RandomBreedImageUrl = new("https://dog.ceo/api/breeds/image/random");

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var client = httpClientFactory.CreateClient(HttpClientName);
            using var response = await client
                .GetAsync(RandomBreedImageUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);
            sw.Stop();
            var code = (int)response.StatusCode;
            var detail = $"http={code} elapsed_ms={sw.ElapsedMilliseconds}";
            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy(detail)
                : HealthCheckResult.Unhealthy($"dog_ceo_api fail {detail}");
        }
        catch (Exception ex)
        {
            sw.Stop();
            return HealthCheckResult.Unhealthy($"dog_ceo_api error elapsed_ms={sw.ElapsedMilliseconds}", ex);
        }
    }
}
