namespace Jarvis.Domain.Shared.Messaging;

/// <summary>
/// The interface for query
/// </summary>
public interface IQuery : IMessage
{

}

/// <summary>
/// The interface for query with Id
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IQuery<T> : IQuery, IMessage<T>
{

}