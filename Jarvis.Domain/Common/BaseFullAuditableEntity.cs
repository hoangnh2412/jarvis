using Jarvis.Domain.Common.Interfaces;

namespace Jarvis.Domain.Common;

public class BaseFullAuditableEntity<T> : IEntity<T>, ILogCreatedEntity, ILogUpdatedEntity, ILogDeletedEntity<T>
{
    public T Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    public T DeletedId { get; set; }
}