namespace Jarvis.Domain.Common.Interfaces;

/// <summary>
/// The interface abstract entities when using ORM
/// </summary>
public interface IEntity
{
}

public interface IEntityKey : IEntity
{
    public Guid Key { get; set; }
}

/// <summary>
/// The interface abstract entities with generic data type of field Id when using ORM
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IEntity<T> : IEntity
{
    T Id { get; set; }
}

public interface IEntityKey<T> : IEntityKey
{
    T Id { get; set; }
}