namespace Jarvis.OpenTelemetry.HostedServices;

/// <summary>
/// Distributed context carried in a message payload or produced when publishing.
/// </summary>
public sealed class MessageTelemetryContext
{
    public string? TraceId { get; init; }

    public string? SpanId { get; init; }

    public string? CorrelationId { get; init; }

    public string? TraceParent { get; init; }

    public string? MessageId { get; init; }

    public static MessageTelemetryContext Empty { get; } = new();
}
