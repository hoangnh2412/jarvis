using Jarvis.Application.Contracts.Commands;
using Jarvis.Domain.Shared.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.Application.Commands;

public class CommandDispatcher(IServiceProvider serviceProvider) : ICommandDispatcher
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    public void Dispatch<TCommand>(TCommand command) where TCommand : ICommand
    {
        var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();
        handler.Handle(command);
    }

    public TResult Dispatch<TCommand, TResult>(TCommand command) where TCommand : ICommand
    {
        var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand, TResult>>();
        return handler.Handle(command);
    }
}