namespace Jarvis.Domain.Entities;

/// <summary>
/// The interface abstract entities with auditing Modification
/// </summary>
public interface ILogUpdatedEntity
{
    DateTime UpdatedAt { get; set; }

    Guid UpdatedBy { get; set; }
}