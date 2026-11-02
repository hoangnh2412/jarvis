using Platform.OrgStructure.Models;

namespace Platform.OrgStructure.Services;

public sealed record OrgUnitTypeDescriptor(Guid Id, string Code, string Name, string Icon, int SortOrder);

public sealed record OrgNamedCatalogDescriptor(Guid Id, string Code, string Name, int SortOrder);

public interface IOrgUnitTypeService
{
    Task<IReadOnlyList<OrgUnitTypeDescriptor>> ListAsync(Guid orgId, CancellationToken cancellationToken);

    Task<OrgUnitTypeDescriptor> CreateAsync(Guid orgId, string code, string name, string icon, CancellationToken cancellationToken);

    Task<OrgUnitTypeDescriptor> UpdateAsync(Guid id, string code, string name, string icon, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

public interface IJobGradeService
{
    Task<IReadOnlyList<OrgNamedCatalogDescriptor>> ListAsync(Guid orgId, CancellationToken cancellationToken);
}

public interface IJobPositionService
{
    Task<IReadOnlyList<OrgNamedCatalogDescriptor>> ListAsync(Guid orgId, CancellationToken cancellationToken);
}

public interface IJobTitleCatalogService
{
    Task<IReadOnlyList<JobTitleDescriptor>> ListAsync(Guid orgId, CancellationToken cancellationToken);
}

public interface ILegalEntityProfileService
{
    Task<LegalEntityApiModel?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<LegalEntityApiModel>> ListAsync(Guid orgId, CancellationToken cancellationToken);

    Task SaveProfileAsync(Guid id, LegalEntityProfilePayload? profile, CancellationToken cancellationToken);
}

public sealed record OrgUnitMemberDto(
    Guid Id,
    Guid UserId,
    string UserName,
    string? DisplayName,
    Guid JobPositionId,
    string JobPositionCode,
    string JobPositionName,
    Guid JobGradeId,
    string JobGradeCode,
    string JobGradeName,
    Guid? JobTitleId,
    string? JobTitleCode,
    string? JobTitleName,
    bool IsActive,
    Guid OrganizationalUnitId);

public sealed record OrgUnitMemberCreateInput(Guid UserId, Guid JobPositionId, Guid JobGradeId, Guid? JobTitleId);

public sealed record OrgUnitMemberUpdateInput(Guid JobPositionId, Guid JobGradeId, Guid? JobTitleId);

public sealed record OrgUnitMemberCount(Guid OrganizationalUnitId, int ActiveCount, int TotalCount);

public interface IOrgUnitMemberService
{
    Task<IReadOnlyList<OrgUnitMemberCount>> CountByOrgAsync(Guid orgId, CancellationToken cancellationToken);

    Task<IReadOnlyList<OrgUnitMemberDto>> ListByUnitAsync(Guid orgId, Guid organizationalUnitId, CancellationToken cancellationToken);

    Task<OrgUnitMemberDto> CreateAsync(Guid orgId, Guid organizationalUnitId, OrgUnitMemberCreateInput input, CancellationToken cancellationToken);

    Task<OrgUnitMemberDto> UpdateAsync(Guid id, OrgUnitMemberUpdateInput input, CancellationToken cancellationToken);

    Task<OrgUnitMemberDto> SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken);
}
