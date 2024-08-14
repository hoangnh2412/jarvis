using Jarvis.Domain.Common.Interfaces;

namespace Jarvis.Domain.Common;

public class BaseBasicEntity : IEntity, ILogCreatedEntity
{
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
}

public class BaseBasicEntity<T> : BaseBasicEntity, IEntity<T>
{
    public T Id { get; set; }
}