using System.Diagnostics;
using Jarvis.OpenTelemetry.Abstractions;
using Jarvis.OpenTelemetry.SemanticConventions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jarvis.OpenTelemetry.HostedServices;

/// <summary>
/// Shared trace, log scope, and metric recording for hosted workers and messaging bases.
/// </summary>
internal static class HostedServiceTelemetry
{
    private static readonly Dictionary<string, ActivitySource> ActivitySources = new(StringComparer.Ordinal);

    public static async Task RunAsync(
        IServiceScopeFactory scopeFactory,
        ILogger logger,
        string activitySourceName,
        string operationName,
        ActivityKind activityKind,
        MessageTelemetryContext? messageContext,
        Func<IServiceProvider, Activity?, CancellationToken, Task> executeAsync,
        Action<Activity?, Dictionary<string, object?>>? configureLogScope = null,
        Action<Activity?>? configureActivity = null,
        string? messagingSystem = null,
        string? messagingDestination = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentException.ThrowIfNullOrWhiteSpace(activitySourceName);
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);
        ArgumentNullException.ThrowIfNull(executeAsync);

        var activitySource = ResolveActivitySource(activitySourceName);
        var parentContext = ResolveParentContext(messageContext);
        var started = Stopwatch.GetTimestamp();

        using var activity = activitySource.StartActivity(operationName, activityKind, parentContext);

        var logScope = BuildLogScope(operationName, messageContext, activity);
        configureLogScope?.Invoke(activity, logScope);
        ApplyMessagingTags(activity, activityKind, messagingSystem, messagingDestination, messageContext);
        configureActivity?.Invoke(activity);

        var success = false;
        await using var scope = scopeFactory.CreateAsyncScope();
        await ApplyEnrichersAsync(scope.ServiceProvider, activity, logScope).ConfigureAwait(false);

        using (logger.BeginScope(logScope))
        {
            try
            {
                await executeAsync(scope.ServiceProvider, activity, cancellationToken).ConfigureAwait(false);
                activity?.SetStatus(ActivityStatusCode.Ok);
                success = true;
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.AddException(ex);
                throw;
            }
            finally
            {
                var elapsedSeconds = Stopwatch.GetElapsedTime(started).TotalSeconds;
                HostedServiceTelemetryMetrics.RecordOperationDuration(operationName, elapsedSeconds, success);
            }
        }
    }

    public static MessageTelemetryContext CreateOutboundContext(Activity? activity, string? correlationId = null)
    {
        if (activity is null)
            return MessageTelemetryContext.Empty;

        var traceId = activity.TraceId.ToString();
        var spanId = activity.SpanId.ToString();
        return new MessageTelemetryContext
        {
            TraceId = traceId,
            SpanId = spanId,
            CorrelationId = correlationId ?? traceId,
            TraceParent = activity.Id,
        };
    }

    public static MessageTelemetryContext ExtractFromCarrier(object message, string? messageId = null)
    {
        if (message is IMessageTelemetryCarrier carrier)
        {
            return new MessageTelemetryContext
            {
                TraceId = carrier.TraceId,
                CorrelationId = carrier.CorrelationId,
                TraceParent = carrier.TraceParent,
                MessageId = messageId,
            };
        }

        return new MessageTelemetryContext { MessageId = messageId };
    }

    private static ActivitySource ResolveActivitySource(string name)
    {
        lock (ActivitySources)
        {
            if (!ActivitySources.TryGetValue(name, out var source))
            {
                source = new ActivitySource(name);
                ActivitySources[name] = source;
            }

            return source;
        }
    }

    private static ActivityContext ResolveParentContext(MessageTelemetryContext? context)
    {
        if (context is null)
            return default;

        if (!string.IsNullOrWhiteSpace(context.TraceParent)
            && ActivityContext.TryParse(context.TraceParent, null, out var fromTraceParent))
        {
            return fromTraceParent;
        }

        if (TryParseTraceId(context.TraceId, out var traceId))
        {
            var spanId = TryParseSpanId(context.SpanId, out var parsedSpanId)
                ? parsedSpanId
                : ActivitySpanId.CreateRandom();
            return new ActivityContext(traceId, spanId, ActivityTraceFlags.Recorded);
        }

        return default;
    }

    private static bool TryParseTraceId(string? value, out ActivityTraceId traceId)
    {
        traceId = default;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        try
        {
            traceId = ActivityTraceId.CreateFromString(value);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private static bool TryParseSpanId(string? value, out ActivitySpanId spanId)
    {
        spanId = default;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        try
        {
            spanId = ActivitySpanId.CreateFromString(value);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private static Dictionary<string, object?> BuildLogScope(
        string operationName,
        MessageTelemetryContext? messageContext,
        Activity? activity)
    {
        var scope = new Dictionary<string, object?>
        {
            [TelemetryContextAttributes.OperationName] = operationName,
        };

        if (!string.IsNullOrWhiteSpace(messageContext?.CorrelationId))
            scope[TelemetryContextAttributes.CorrelationId] = messageContext.CorrelationId;

        if (!string.IsNullOrWhiteSpace(messageContext?.TraceId))
            scope[TelemetryContextAttributes.TraceId] = messageContext.TraceId;

        if (!string.IsNullOrWhiteSpace(messageContext?.TraceParent))
            scope[TelemetryContextAttributes.TraceParent] = messageContext.TraceParent;

        if (!string.IsNullOrWhiteSpace(messageContext?.MessageId))
            scope[MessagingAttributes.MessageId] = messageContext.MessageId;

        if (activity is not null)
        {
            scope[TelemetryContextAttributes.TraceId] = activity.TraceId.ToString();
            scope[TelemetryContextAttributes.SpanId] = activity.SpanId.ToString();
            scope[TelemetryContextAttributes.TraceParent] = activity.Id;
        }

        return scope;
    }

    private static void ApplyMessagingTags(
        Activity? activity,
        ActivityKind kind,
        string? messagingSystem,
        string? destination,
        MessageTelemetryContext? messageContext)
    {
        if (activity is null)
            return;

        activity.SetTag(TelemetryContextAttributes.OperationName, activity.OperationName);

        if (!string.IsNullOrWhiteSpace(messagingSystem))
            activity.SetTag(MessagingAttributes.System, messagingSystem);

        if (!string.IsNullOrWhiteSpace(destination))
            activity.SetTag(MessagingAttributes.DestinationName, destination);

        if (!string.IsNullOrWhiteSpace(messageContext?.MessageId))
            activity.SetTag(MessagingAttributes.MessageId, messageContext.MessageId);

        if (!string.IsNullOrWhiteSpace(messageContext?.CorrelationId))
            activity.SetTag(MessagingAttributes.ConversationId, messageContext.CorrelationId);

        activity.SetTag(
            MessagingAttributes.Operation,
            kind switch
            {
                ActivityKind.Producer => MessagingAttributes.OperationPublish,
                ActivityKind.Consumer => MessagingAttributes.OperationProcess,
                _ => "execute",
            });
    }

    private static async Task ApplyEnrichersAsync(
        IServiceProvider serviceProvider,
        Activity? activity,
        Dictionary<string, object?> logScope)
    {
        foreach (var enricher in serviceProvider.GetServices<IEnrichTraceService>())
        {
            foreach (var item in await enricher.ExtractAsync().ConfigureAwait(false))
            {
                activity?.SetTag(item.Key, item.Value);
                logScope.TryAdd(item.Key, item.Value);
            }
        }

        foreach (var enricher in serviceProvider.GetServices<IEnrichLogService>())
        {
            foreach (var item in await enricher.ExtractAsync().ConfigureAwait(false))
                logScope.TryAdd(item.Key, item.Value);
        }
    }
}
