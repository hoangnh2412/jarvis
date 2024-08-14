namespace Jarvis.Domain.Common.Interfaces;

/// <summary>
/// The interface abstract entities with auditing Creation
/// </summary>
public interface ILogCreatedEntity
{
    DateTime CreatedAt { get; set; }
    Guid CreatedBy { get; set; }
}