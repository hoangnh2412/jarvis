namespace Jarvis.Domain.Common.Interfaces;

/// <summary>
/// The interface abstract entities with auditing Modification
/// </summary>
public interface ILogUpdatedEntity
{
    DateTime? UpdatedAt { get; set; }
    Guid? UpdatedBy { get; set; }
}