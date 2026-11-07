using Platform.Tenants;

namespace Platform.Host.Entities;

public sealed class Tenant : ITenantCatalogEntity
{
    public Guid Id { get; set; }

    public Guid? ParentId { get; set; }

    public string ConnectionString { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
