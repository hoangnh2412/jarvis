using Jarvis.Domain.Shared.Messaging;

namespace Jarvis.Application.Contracts.Commands;

/// <summary>
/// Defines an asynchronous dispatcher for handling commands.
/// </summary>
public interface IAsyncCommandDispatcher
{
    /// <summary>
    /// Asynchronously dispatches the specified command.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to be dispatched.</typeparam>
    /// <param name="command">The command to dispatch.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DispatchAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand;

    /// <summary>
    /// Asynchronously dispatches the specified command and returns a result.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to be dispatched.</typeparam>
    /// <typeparam name="TResult">The type of result returned after dispatching the command.</typeparam>
    /// <param name="command">The command to dispatch.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation, with a result of the specified type.</returns>
    Task<TResult> DispatchAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand;
}