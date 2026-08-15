using System.Diagnostics.Metrics;
using System.Reflection;
using Jarvis.OpenTelemetry.SemanticConventions;

namespace Jarvis.OpenTelemetry.HostedServices;

internal static class HostedServiceTelemetryMetrics
{
    private static readonly Meter Meter = new(ResolveMeterName());

    private static readonly Counter<long> WorkerExecutions = Meter.CreateCounter<long>(
        "jarvis.worker.executions",
        description: "Number of worker job executions");

    private static readonly Counter<long> MessagesPublished = Meter.CreateCounter<long>(
        "jarvis.messaging.messages.published",
        description: "Number of messages published");

    private static readonly Counter<long> MessagesConsumed = Meter.CreateCounter<long>(
        "jarvis.messaging.messages.consumed",
        description: "Number of messages consumed");

    private static readonly Histogram<double> OperationDurationSeconds = Meter.CreateHistogram<double>(
        "jarvis.hosted_service.operation.duration",
        unit: "s",
        description: "Duration of hosted service operations");

    public static void RecordWorkerExecution(string workerName, bool success)
    {
        WorkerExecutions.Add(
            1,
            new KeyValuePair<string, object?>(TelemetryContextAttributes.WorkerName, workerName),
            new KeyValuePair<string, object?>("success", success));
    }

    public static void RecordMessagePublished(string? messagingSystem, string? destination)
    {
        MessagesPublished.Add(
            1,
            new KeyValuePair<string, object?>(MessagingAttributes.System, messagingSystem),
            new KeyValuePair<string, object?>(MessagingAttributes.DestinationName, destination));
    }

    public static void RecordMessageConsumed(string? messagingSystem, string? destination, bool success)
    {
        MessagesConsumed.Add(
            1,
            new KeyValuePair<string, object?>(MessagingAttributes.System, messagingSystem),
            new KeyValuePair<string, object?>(MessagingAttributes.DestinationName, destination),
            new KeyValuePair<string, object?>("success", success));
    }

    public static void RecordOperationDuration(string operationName, double durationSeconds, bool success)
    {
        OperationDurationSeconds.Record(
            durationSeconds,
            new KeyValuePair<string, object?>(TelemetryContextAttributes.OperationName, operationName),
            new KeyValuePair<string, object?>("success", success));
    }

    private static string ResolveMeterName()
        => Assembly.GetEntryAssembly()?.GetName().Name ?? typeof(HostedServiceTelemetryMetrics).Assembly.GetName().Name!;
}
