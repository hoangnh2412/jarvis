using Jarvis.Domain.Shared.Messaging;

namespace Jarvis.Application.Contracts.Commands;

/// <summary>
/// Defines a handler for a command without a result.
/// </summary>
/// <typeparam name="TCommand">The type of command to be handled.</typeparam>
public interface ICommandHandler<TCommand> where TCommand : ICommand
{
    /// <summary>
    /// Handles the specified command.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    void Handle(TCommand command);
}

/// <summary>
/// Defines a handler for a command that returns a result.
/// </summary>
/// <typeparam name="TCommand">The type of command to be handled.</typeparam>
/// <typeparam name="TResult">The type of result returned after handling the command.</typeparam>
public interface ICommandHandler<TCommand, TResult> where TCommand : ICommand
{
    /// <summary>
    /// Handles the specified command and returns a result.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <returns>The result of handling the command.</returns>
    TResult Handle(TCommand command);
}