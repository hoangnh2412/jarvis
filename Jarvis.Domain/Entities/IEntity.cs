namespace Jarvis.Domain.Entities;

/// <summary>
/// The interface abstract entities when using ORM
/// </summary>
public interface IEntity
{
}

/// <summary>
/// The interface abstract entities with generic data type of field Id when using ORM
/// </summary>
/// <typeparam name="TKey"></typeparam>
public interface IEntity<TKey> : IEntity
{
    TKey Id { get; set; }
}
