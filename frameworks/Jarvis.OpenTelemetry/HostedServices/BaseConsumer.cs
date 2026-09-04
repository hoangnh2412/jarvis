using System.Diagnostics;
using System.Reflection;
using Jarvis.OpenTelemetry.SemanticConventions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jarvis.OpenTelemetry.HostedServices;

/// <summary>
/// Message consumer loop without broker-specific dependencies.
/// Subclass implements <see cref="ReceiveAsync"/> and <see cref="ProcessMessageAsync"/>.
/// </summary>
public abstract class BaseConsumer<TMessage>(
    IServiceScopeFactory scopeFactory,
    ILogger logger) : BackgroundService where TMessage : class
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger _logger = logger;

    /// <summary>Broker label for spans and metrics (e.g. rabbitmq, kafka, redis).</summary>
    protected abstract string MessagingSystem { get; }

    /// <summary>Queue or topic name.</summary>
    protected abstract string DestinationName { get; }

    protected virtual string ConsumerName => GetType().Name;

    protected virtual string ActivitySourceName =>
        Assembly.GetEntryAssembly()?.GetName().Name ?? typeof(BaseConsumer<TMessage>).Assembly.GetName().Name!;

    /// <summary>Delay when <see cref="ReceiveAsync"/> returns no message.</summary>
    protected virtual TimeSpan IdleDelay => TimeSpan.FromMilliseconds(200);

    /// <summary>Receive the next message from the underlying transport.</summary>
    protected abstract Task<TMessage?> ReceiveAsync(CancellationToken cancellationToken);

    /// <summary>Process a single message inside the scoped DI container.</summary>
    protected abstract Task ProcessMessageAsync(
        TMessage message,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);

    /// <summary>Extract distributed context embedded in the message payload.</summary>
    protected virtual MessageTelemetryContext ExtractTelemetryContext(TMessage message, string? messageId = null)
        => HostedServiceTelemetry.ExtractFromCarrier(message, messageId);

    /// <summary>Optional message id used when the payload does not implement <see cref="IMessageTelemetryCarrier"/>.</summary>
    protected virtual string? ResolveMessageId(TMessage message) => null;

    protected virtual void ConfigureTelemetry(
        TMessage message,
        Activity? activity,
        Dictionary<string, object?> logScope)
    {
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            TMessage? message;
            try
            {
                message = await ReceiveAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Consumer {ConsumerName} failed to receive from {Destination}", ConsumerName, DestinationName);
                await Task.Delay(IdleDelay, stoppingToken).ConfigureAwait(false);
                continue;
            }

            if (message is null)
            {
                await Task.Delay(IdleDelay, stoppingToken).ConfigureAwait(false);
                continue;
            }

            await ProcessOneMessageAsync(message, stoppingToken).ConfigureAwait(false);
        }
    }

    private async Task ProcessOneMessageAsync(TMessage message, CancellationToken cancellationToken)
    {
        var messageId = ResolveMessageId(message);
        var telemetryContext = ExtractTelemetryContext(message, messageId);
        var operationName = $"{ConsumerName}.process";
        var success = false;

        try
        {
            await HostedServiceTelemetry.RunAsync(
                _scopeFactory,
                _logger,
                ActivitySourceName,
                operationName,
                ActivityKind.Consumer,
                telemetryContext,
                executeAsync: async (serviceProvider, _, ct) =>
                {
                    await ProcessMessageAsync(message, serviceProvider, ct).ConfigureAwait(false);
                },
                configureLogScope: (activity, logScope) => ConfigureTelemetry(message, activity, logScope),
                messagingSystem: MessagingSystem,
                messagingDestination: DestinationName,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Consumer {ConsumerName} failed to process message from {Destination}",
                ConsumerName,
                DestinationName);
        }
        finally
        {
            HostedServiceTelemetryMetrics.RecordMessageConsumed(MessagingSystem, DestinationName, success);
        }
    }
}
