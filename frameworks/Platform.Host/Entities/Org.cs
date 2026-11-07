using Platform.Orgs;

namespace Platform.Host.Entities;

public sealed class Org : IOrgManagementEntity
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid? ParentId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
