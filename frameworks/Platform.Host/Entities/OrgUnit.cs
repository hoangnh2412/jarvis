using Platform.OrgStructure;

namespace Platform.Host.Entities;

public sealed class OrgUnit : IOrganizationalUnitEntity
{
    public Guid Id { get; set; }

    public Guid OrgId { get; set; }

    public Guid TenantId { get; set; }

    public Guid? ParentId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string UnitType { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public Guid? LegalEntityId { get; set; }

    public string Path { get; set; } = string.Empty;
}
