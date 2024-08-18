using Jarvis.Domain.Shared.Messaging;

namespace Jarvis.Application.Contracts.Commands;

/// <summary>
/// Interface for command handler class without result
/// Used where there is command from user/other system that change system state.
/// </summary>
/// <typeparam name="TCommand">Type of the command</typeparam>
public interface IAsyncCommandHandler<TCommand> where TCommand : ICommand
{
    /// <summary>
    /// Handle command asynchronously
    /// </summary>
    /// <param name="command">The command to handle</param>
    /// <param name="cancellationToken">Cancellation token to cancel async methods (if any)</param>
    Task HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for command handler class result
/// Used where there is command from user/other system that change system state.
/// </summary>
/// <typeparam name="TCommand">Type of the command</typeparam>
/// <typeparam name="TResult">Type of the result</typeparam>
public interface IAsyncCommandHandler<TCommand, TResult> where TCommand : ICommand
{
    /// <summary>
    /// Handle command asynchronously
    /// </summary>
    /// <param name="command">The command to handle</param>
    /// <param name="cancellationToken">Cancellation token to cancel async methods (if any)</param>
    /// <returns>The expect results</returns>
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}