using Platform.OrgStructure.Models;

namespace Platform.OrgStructure.Services;

public interface IOrgStructureManager
{
    Task<OrganizationalUnitDescriptor> CreateUnitAsync(
        CreateOrganizationalUnitInput input,
        CancellationToken cancellationToken = default);

    Task<OrganizationalUnitDescriptor> UpdateUnitAsync(
        Guid id,
        UpdateOrganizationalUnitInput input,
        CancellationToken cancellationToken = default);

    Task<OrganizationalUnitDescriptor> MoveUnitAsync(
        Guid id,
        Guid? parentId,
        CancellationToken cancellationToken = default);

    Task<OrganizationalUnitDescriptor> SetUnitActiveAsync(
        Guid id,
        bool isActive,
        CancellationToken cancellationToken = default);

    Task<OrganizationalUnitDescriptor> GetUnitAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrganizationalUnitDescriptor>> ListUnitsAsync(
        ListOrganizationalUnitsQuery query,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrganizationalUnitDescriptor>> GetDescendantsAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<LegalEntityDescriptor> CreateLegalEntityAsync(
        CreateLegalEntityInput input,
        CancellationToken cancellationToken = default);

    Task<LegalEntityDescriptor> UpdateLegalEntityAsync(
        Guid id,
        UpdateLegalEntityInput input,
        CancellationToken cancellationToken = default);

    Task<LegalEntityDescriptor> SetLegalEntityActiveAsync(
        Guid id,
        bool isActive,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LegalEntityDescriptor>> ListLegalEntitiesAsync(
        Guid orgId,
        CancellationToken cancellationToken = default);

    Task<JobTitleDescriptor> CreateJobTitleAsync(
        CreateJobTitleInput input,
        CancellationToken cancellationToken = default);

    Task<JobTitleDescriptor> UpdateJobTitleAsync(
        Guid id,
        UpdateJobTitleInput input,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<JobTitleDescriptor>> ListJobTitlesAsync(
        Guid orgId,
        CancellationToken cancellationToken = default);

    Task<PositionDescriptor> CreatePositionAsync(
        CreatePositionInput input,
        CancellationToken cancellationToken = default);

    Task<PositionDescriptor> UpdatePositionAsync(
        Guid id,
        UpdatePositionInput input,
        CancellationToken cancellationToken = default);

    Task<PositionDescriptor> SetPositionActiveAsync(
        Guid id,
        bool isActive,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PositionDescriptor>> ListPositionsAsync(
        Guid orgId,
        CancellationToken cancellationToken = default);

    OrgStructureTemplatePreview PreviewTemplate(string templateCode, string rootName);

    Task<IReadOnlyList<OrganizationalUnitDescriptor>> ApplyTemplateAsync(
        ApplyOrgStructureTemplateInput input,
        CancellationToken cancellationToken = default);
}
