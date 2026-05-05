using Jarvis.HealthChecks;

namespace Sample.Health;

/// <summary>Example: supply queue depth / timing text for detailed and readiness health JSON.</summary>
public sealed class SampleHealthIntegrationMetricsProvider : IJarvisHealthIntegrationMetricsProvider
{
    public Task<string> GetMetricsDescriptionAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult("queue_depth=0; disk_check_response_ms=~0");
}
