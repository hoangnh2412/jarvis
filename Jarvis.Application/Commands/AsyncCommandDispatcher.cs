using Jarvis.Application.Contracts.Commands;
using Jarvis.Domain.Shared.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.Application.Commands;

/// <summary>
/// Implements IAsyncCommandDispatcher
/// </summary>
/// <param name="serviceProvider"></param>
public class AsyncCommandDispatcher(IServiceProvider serviceProvider) : IAsyncCommandDispatcher
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    /// <summary>
    /// Asynchronously dispatches the specified command.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to be dispatched.</typeparam>
    /// <param name="command">The command to dispatch.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task DispatchAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand
    {
        var handler = _serviceProvider.GetRequiredService<IAsyncCommandHandler<TCommand>>();
        await handler.HandleAsync(command, cancellationToken);
    }

    public async Task<TResult> DispatchAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand
    {
        var handler = _serviceProvider.GetRequiredService<IAsyncCommandHandler<TCommand, TResult>>();
        return await handler.HandleAsync(command, cancellationToken);
    }
}