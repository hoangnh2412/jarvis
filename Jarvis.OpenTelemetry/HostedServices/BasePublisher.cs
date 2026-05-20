using System.Diagnostics;
using System.Reflection;
using Jarvis.OpenTelemetry.SemanticConventions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jarvis.OpenTelemetry.HostedServices;

/// <summary>
/// Message publisher without broker-specific dependencies.
/// Subclass implements transport send and payload enrichment.
/// </summary>
public abstract class BasePublisher<TMessage>(
    IServiceScopeFactory scopeFactory,
    ILogger logger) where TMessage : class
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger _logger = logger;

    protected abstract string MessagingSystem { get; }

    protected abstract string DestinationName { get; }

    protected virtual string PublisherName => GetType().Name;

    protected virtual string ActivitySourceName =>
        Assembly.GetEntryAssembly()?.GetName().Name ?? typeof(BasePublisher<TMessage>).Assembly.GetName().Name!;

    /// <summary>
    /// Publishes a message inside a producer span and injects trace context into the payload.
    /// </summary>
    public virtual async Task PublishAsync(TMessage message, CancellationToken cancellationToken = default)
    {
        var operationName = $"{PublisherName}.publish";

        try
        {
            await HostedServiceTelemetry.RunAsync(
                _scopeFactory,
                _logger,
                ActivitySourceName,
                operationName,
                ActivityKind.Producer,
                messageContext: null,
                executeAsync: async (serviceProvider, activity, ct) =>
                {
                    var outboundContext = CreateOutboundTelemetryContext(activity);
                    ApplyTelemetryToMessage(message, outboundContext);
                    ConfigureTelemetry(message, activity, outboundContext);
                    await PublishCoreAsync(message, serviceProvider, ct).ConfigureAwait(false);
                },
                messagingSystem: MessagingSystem,
                messagingDestination: DestinationName,
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Publisher {PublisherName} failed for destination {Destination}",
                PublisherName,
                DestinationName);
            throw;
        }
        finally
        {
            HostedServiceTelemetryMetrics.RecordMessagePublished(MessagingSystem, DestinationName);
        }
    }

    protected abstract Task PublishCoreAsync(
        TMessage message,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);

    /// <summary>Write <see cref="MessageTelemetryContext"/> fields into the outbound message payload.</summary>
    protected abstract void ApplyTelemetryToMessage(TMessage message, MessageTelemetryContext context);

    protected virtual MessageTelemetryContext CreateOutboundTelemetryContext(Activity? activity, string? correlationId = null)
        => HostedServiceTelemetry.CreateOutboundContext(activity, correlationId);

    protected virtual void ConfigureTelemetry(
        TMessage message,
        Activity? activity,
        MessageTelemetryContext context)
    {
    }
}
