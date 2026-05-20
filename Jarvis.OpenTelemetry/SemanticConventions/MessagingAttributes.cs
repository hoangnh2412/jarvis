namespace Jarvis.OpenTelemetry.SemanticConventions;

/// <summary>
/// Messaging-related attribute names for traces, logs, and metrics.
/// See https://opentelemetry.io/docs/specs/semconv/registry/messaging/
/// </summary>
public static class MessagingAttributes
{
    public const string System = "messaging.system";
    public const string Operation = "messaging.operation";
    public const string DestinationName = "messaging.destination.name";
    public const string MessageId = "messaging.message.id";
    public const string ConversationId = "messaging.message.conversation_id";
    public const string BodySize = "messaging.message.body.size";

    public const string OperationPublish = "publish";
    public const string OperationReceive = "receive";
    public const string OperationProcess = "process";
}
