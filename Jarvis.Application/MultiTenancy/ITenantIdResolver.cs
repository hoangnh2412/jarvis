namespace Jarvis.Application.MultiTenancy;

/// <summary>
/// The interface abstract tenant identification
/// </summary>
public interface ITenantIdResolver
{
    Guid GetTenantId();
}