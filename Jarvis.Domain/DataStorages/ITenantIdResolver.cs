namespace Jarvis.Domain.DataStorages;

/// <summary>
/// The interface abstract tenant identification
/// </summary>
public interface ITenantIdResolver
{
    Guid Resolve();
    
    Task<Guid> ResolveAsync();
}