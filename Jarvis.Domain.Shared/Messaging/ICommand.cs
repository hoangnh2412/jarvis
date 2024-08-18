namespace Jarvis.Domain.Shared.Messaging;

/// <summary>
/// The interface for command
/// </summary>
public interface ICommand : IMessage
{

}

/// <summary>
/// The interface for Command with Id
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ICommand<T> : ICommand, IMessage<T>
{

}