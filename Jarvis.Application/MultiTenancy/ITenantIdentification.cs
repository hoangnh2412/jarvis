namespace Jarvis.Application.MultiTenancy;

/// <summary>
/// The interface abstract tenant identification
/// </summary>
public interface ITenantIdentification
{
    Task<Guid> GetCurrentAsync();
}