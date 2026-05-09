using System.Diagnostics.Metrics;

namespace Sample.Telemetry;

/// <summary>
/// Custom metrics for <see cref="Controllers.User1Controller"/> (same <see cref="Meter"/> name as Jarvis <c>AddMeter("Sample")</c>).
/// </summary>
public static class SampleUserV1Metrics
{
    private static readonly Meter Meter = new("Sample", "1.0.0");

    public static readonly Counter<long> DbQueries = Meter.CreateCounter<long>(
        "sample.users.v1.db.queries",
        unit: "{query}",
        description: "EF Core queries executed from Users v1 demo endpoint.");

    public static readonly Counter<long> RedisOperations = Meter.CreateCounter<long>(
        "sample.users.v1.redis.operations",
        unit: "{operation}",
        description: "Redis commands executed from Users v1 demo endpoint.");

    public static readonly Histogram<double> HandlerDurationSeconds = Meter.CreateHistogram<double>(
        "sample.users.v1.handler.duration",
        unit: "s",
        description: "Wall time for Users v1 GET handler (DB + Redis demo).");
}
