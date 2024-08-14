namespace Jarvis.Domain.Common.Interfaces;

/// <summary>
/// The interface abstract entities when use multi-tenant
/// </summary>
public interface ITenantEntity
{
    Guid TenantId { get; set; }
}