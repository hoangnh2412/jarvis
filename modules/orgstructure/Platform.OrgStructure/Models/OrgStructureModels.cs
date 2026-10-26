namespace Platform.OrgStructure.Models;

public sealed class OrganizationalUnitDescriptor
{
    public Guid Id { get; init; }

    public Guid OrgId { get; init; }

    public Guid TenantId { get; init; }

    public Guid? ParentId { get; init; }

    public string Code { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string UnitType { get; init; } = string.Empty;

    public int SortOrder { get; init; }

    public bool IsActive { get; init; }

    public Guid? LegalEntityId { get; init; }

    public string Path { get; init; } = string.Empty;
}

public sealed class LegalEntityDescriptor
{
    public Guid Id { get; init; }

    public Guid OrgId { get; init; }

    public string Code { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string? TaxId { get; init; }

    public bool IsActive { get; init; }
}

public sealed class JobTitleDescriptor
{
    public Guid Id { get; init; }

    public Guid OrgId { get; init; }

    public string Code { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;
}

public sealed class PositionDescriptor
{
    public Guid Id { get; init; }

    public Guid OrgId { get; init; }

    public Guid OrganizationalUnitId { get; init; }

    public Guid? JobTitleId { get; init; }

    public Guid? ReportsToPositionId { get; init; }

    public bool IsActive { get; init; }
}

public sealed record CreateOrganizationalUnitInput(
    Guid OrgId,
    Guid TenantId,
    string Code,
    string Name,
    string UnitType,
    Guid? ParentId,
    int SortOrder,
    Guid? LegalEntityId);

public sealed record UpdateOrganizationalUnitInput(
    string Code,
    string Name,
    string UnitType,
    int SortOrder,
    Guid? LegalEntityId);

public sealed record ListOrganizationalUnitsQuery(
    Guid OrgId,
    string? UnitType,
    Guid? LegalEntityId,
    bool? IsActive);

public sealed record CreateLegalEntityInput(Guid OrgId, string Code, string Name, string? TaxId);

public sealed record UpdateLegalEntityInput(string Code, string Name, string? TaxId);

public sealed record CreateJobTitleInput(Guid OrgId, string Code, string Name);

public sealed record UpdateJobTitleInput(string Code, string Name);

public sealed record CreatePositionInput(
    Guid OrgId,
    Guid OrganizationalUnitId,
    Guid? JobTitleId,
    Guid? ReportsToPositionId);

public sealed record UpdatePositionInput(Guid? JobTitleId, Guid? ReportsToPositionId);

public sealed record OrgStructureTemplateNode(
    string Code,
    string Name,
    string UnitType,
    string? ParentCode,
    string Path);

public sealed record OrgStructureTemplatePreview(
    string TemplateCode,
    string RootName,
    IReadOnlyList<OrgStructureTemplateNode> Nodes);

public sealed record ApplyOrgStructureTemplateInput(
    Guid OrgId,
    Guid TenantId,
    string TemplateCode,
    string RootName,
    bool Confirmed,
    IReadOnlyList<OrgStructureTemplateNode> Nodes);
