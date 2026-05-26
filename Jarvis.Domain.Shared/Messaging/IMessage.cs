namespace Jarvis.Domain.Shared.Messaging;

public interface IMessage
{

}

public interface IMessage<T> : IMessage
{
    public T Id { get; set; }
}