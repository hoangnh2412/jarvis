using Jarvis.Domain.Common.Interfaces;

namespace Jarvis.Domain.Common;

public class BaseEntity : IEntity
{
}

public class BaseEntity<T> : BaseEntity, IEntity<T>
{
    public T Id { get; set; }
}