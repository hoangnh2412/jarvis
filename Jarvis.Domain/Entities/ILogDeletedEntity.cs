namespace Jarvis.Domain.Entities;

/// <summary>
/// The interface abstract entities with auditing Deletion
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ILogDeletedEntity<T>
{
    DateTime? DeletedAt { get; set; }
    
    Guid? DeletedBy { get; set; }
    
    T DeletedId { get; set; }
}