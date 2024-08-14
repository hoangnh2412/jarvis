using Jarvis.Domain.Common.Interfaces;

namespace Jarvis.Domain.Common;

public class BaseAuditableEntity : IEntity, ILogCreatedEntity, ILogUpdatedEntity
{
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}

public class BaseAuditableEntity<T> : BaseAuditableEntity, IEntity<T>
{
    public T Id { get; set; }
}