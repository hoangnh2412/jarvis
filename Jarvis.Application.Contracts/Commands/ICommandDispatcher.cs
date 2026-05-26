using Jarvis.Domain.Shared.Messaging;

namespace Jarvis.Application.Contracts.Commands;

/// <summary>
/// Defines a dispatcher for handling commands.
/// </summary>
public interface ICommandDispatcher
{
    /// <summary>
    /// Dispatches the specified command.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to be dispatched.</typeparam>
    /// <param name="command">The command to dispatch.</param>
    void Dispatch<TCommand>(TCommand command) where TCommand : ICommand;

    /// <summary>
    /// Dispatches the specified command and returns a result.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to be dispatched.</typeparam>
    /// <typeparam name="TResult">The type of result returned after dispatching the command.</typeparam>
    /// <param name="command">The command to dispatch.</param>
    /// <returns>The result of dispatching the command.</returns>
    TResult Dispatch<TCommand, TResult>(TCommand command) where TCommand : ICommand;
}