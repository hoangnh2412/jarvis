namespace Jarvis.Application.MultiTenancy;

/// <summary>
/// The interface abstract tenant identification
/// </summary>
public interface ITenantIdAccessor
{
    Task<Guid> GetCurrentAsync();
}