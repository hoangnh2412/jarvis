using Platform.DDD.Application.Contracts.Commands;
using Platform.DDD.Domain.Shared.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Platform.DDD.Application.Commands;

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