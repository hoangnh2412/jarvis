namespace Jarvis.OpenTelemetry.HostedServices;

/// <summary>
/// Optional contract for message DTOs that embed distributed context in the payload.
/// </summary>
public interface IMessageTelemetryCarrier
{
    string? TraceId { get; }

    string? CorrelationId { get; }

    string? TraceParent { get; }
}
