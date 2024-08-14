namespace Jarvis.Domain.Entities;

/// <summary>
/// The interface abstract entities with auditing Creation
/// </summary>
public interface ILogCreatedEntity
{
    DateTime CreatedAt { get; set; }
    
    Guid CreatedBy { get; set; }
}