namespace Jarvis.Domain.Entities;

public class BaseEntity : IEntity
{

}

public class BaseEntity<TKey> : BaseEntity, IEntity<TKey>
{
    public required TKey Id { get; set; }
}