using Jarvis.OpenTelemetry.Abstractions;
using Jarvis.OpenTelemetry.Enrichment;
using Jarvis.OpenTelemetry.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace UnitTest.OpenTelemetry;

public sealed class TelemetryEnrichmentTests
{
    [Fact]
    public async Task EnrichLogService_Merges_Sources_Later_Overwrites_Key()
    {
        var enricher = new EnrichLogService(
        [
            new FixedSource(new Dictionary<string, string> { ["a"] = "1", ["b"] = "2" }),
            new FixedSource(new Dictionary<string, string> { ["b"] = "override", ["c"] = "3" }),
        ]);

        var data = await enricher.ExtractAsync();

        Assert.Equal("1", data["a"]);
        Assert.Equal("override", data["b"]);
        Assert.Equal("3", data["c"]);
    }

    [Fact]
    public async Task EnrichTraceService_Merges_Sources()
    {
        var enricher = new EnrichTraceService(
        [
            new FixedSource(new Dictionary<string, string> { ["trace.attr"] = "x" }),
        ]);

        var data = await enricher.ExtractAsync();

        Assert.Equal("x", data["trace.attr"]);
    }

    [Fact]
    public void AddTelemetryEnrichment_Registers_Default_Enrichers()
    {
        var services = new ServiceCollection();
        services.AddTelemetryEnrichment();
        services.AddScoped<IEnrichmentSource>(_ =>
            new FixedSource(new Dictionary<string, string> { ["k"] = "v" }));

        using var provider = services.BuildServiceProvider();
        var log = provider.GetServices<IEnrichLogService>().ToList();
        var trace = provider.GetServices<IEnrichTraceService>().ToList();

        Assert.Single(log);
        Assert.IsType<EnrichLogService>(log[0]);
        Assert.Single(trace);
        Assert.IsType<EnrichTraceService>(trace[0]);
    }

    private sealed class FixedSource(Dictionary<string, string> data) : IEnrichmentSource
    {
        public Task<Dictionary<string, string>> ExtractAsync() => Task.FromResult(data);
    }
}
