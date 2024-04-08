using Jarvis.Domain.Entities;

namespace Jarvis.Application.MultiTenancy;

/// <summary>
/// The interface abstract tenant identification
/// </summary>
public interface ITenantIdentification
{
    Task<Guid> GetCurrentAsync();

    Task<ITenant> GetTenantAsync(string hostname);

    Task<ITenant> GetTenantAsync(Guid tenantId);
}