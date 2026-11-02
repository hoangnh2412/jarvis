using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Platform.OrgStructure;
using Platform.Authentication.Identity.Domain;
using Platform.DDD.Domain.Services;
using Platform.Tenants.Services;
using Platform.OrgStructure.Models;
using Platform.OrgStructure.Services;
using Platform.Orgs.Services;
using Platform.Modules.IdentityAdmin.Services;
namespace Platform.OrgStructure.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/org-structure")]
public sealed class OrgStructureController(
    IOrgStructureManager structure,
    IOrgUnitTypeService unitTypes,
    ILegalEntityProfileService legalEntityProfiles,
    IJobPositionService jobPositions,
    IJobGradeService jobGrades,
    IJobTitleCatalogService jobTitleCatalog,
    IOrgUnitMemberService unitMembers,
    ICurrentTenant<TenantCatalogProfile> currentTenant,
    ICurrentOrg<OrgCatalogOrgProfile> currentOrg,
    RealmUserOperations realmUsers) : ControllerBase
{
    [HttpGet("abac-filter")]
    public IActionResult AbacFilter([FromServices] IOptions<OrgStructureOptions> options) =>
        Ok(new { filterAbacByOrgStructure = options.Value.FilterABACByOrgStructure });

    [HttpGet("unit-types")]
    public async Task<IActionResult> UnitTypes(CancellationToken cancellationToken)
    {
        var scope = await RequireScopeAsync(cancellationToken).ConfigureAwait(false);
        if (scope is null) return ScopeMissing();
        return Ok(await unitTypes.ListAsync(scope.Value.OrgId, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("unit-types")]
    public async Task<IActionResult> CreateUnitType([FromBody] UnitTypeBody body, CancellationToken cancellationToken)
    {
        var scope = await RequireScopeAsync(cancellationToken).ConfigureAwait(false);
        if (scope is null) return ScopeMissing();
        try
        {
            return Ok(await unitTypes.CreateAsync(
                scope.Value.OrgId,
                body.Code ?? string.Empty,
                body.Name ?? string.Empty,
                body.Icon ?? string.Empty,
                cancellationToken).ConfigureAwait(false));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("unit-types/{id:guid}")]
    public async Task<IActionResult> UpdateUnitType(Guid id, [FromBody] UnitTypeBody body, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await unitTypes.UpdateAsync(
                id,
                body.Code ?? string.Empty,
                body.Name ?? string.Empty,
                body.Icon ?? string.Empty,
                cancellationToken).ConfigureAwait(false));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("unit-types/{id:guid}")]
    public async Task<IActionResult> DeleteUnitType(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await unitTypes.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("units")]
    public async Task<IActionResult> ListUnits(
        [FromQuery] string? unitType,
        [FromQuery] Guid? legalEntityId,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        var scope = await RequireScopeAsync(cancellationToken).ConfigureAwait(false);
        if (scope is null) return ScopeMissing();
        var items = await structure.ListUnitsAsync(
            new ListOrganizationalUnitsQuery(scope.Value.OrgId, unitType, legalEntityId, isActive),
            cancellationToken).ConfigureAwait(false);
        return Ok(items);
    }

    [HttpGet("units/{id:guid}/descendants")]
    public async Task<IActionResult> Descendants(Guid id, CancellationToken cancellationToken) =>
        Ok(await structure.GetDescendantsAsync(id, cancellationToken).ConfigureAwait(false));

    [HttpPost("units")]
    public async Task<IActionResult> CreateUnit([FromBody] CreateUnitBody body, CancellationToken cancellationToken)
    {
        var scope = await RequireScopeAsync(cancellationToken).ConfigureAwait(false);
        if (scope is null) return ScopeMissing();
        var created = await structure.CreateUnitAsync(new CreateOrganizationalUnitInput(
            scope.Value.OrgId,
            scope.Value.TenantId,
            body.Code ?? string.Empty,
            body.Name ?? string.Empty,
            body.UnitType ?? string.Empty,
            body.ParentId,
            body.SortOrder,
            body.LegalEntityId), cancellationToken).ConfigureAwait(false);
        return Ok(created);
    }

    [HttpPut("units/{id:guid}")]
    public async Task<IActionResult> UpdateUnit(Guid id, [FromBody] UpdateUnitBody body, CancellationToken cancellationToken)
    {
        var updated = await structure.UpdateUnitAsync(id, new UpdateOrganizationalUnitInput(
            body.Code ?? string.Empty,
            body.Name ?? string.Empty,
            body.UnitType ?? string.Empty,
            body.SortOrder,
            body.LegalEntityId), cancellationToken).ConfigureAwait(false);
        return Ok(updated);
    }

    [HttpPut("units/{id:guid}/parent")]
    public async Task<IActionResult> MoveUnit(Guid id, [FromBody] MoveUnitBody body, CancellationToken cancellationToken) =>
        Ok(await structure.MoveUnitAsync(id, body.ParentId, cancellationToken).ConfigureAwait(false));

    [HttpPatch("units/{id:guid}/active")]
    public async Task<IActionResult> SetUnitActive(Guid id, [FromBody] ActiveBody body, CancellationToken cancellationToken) =>
        Ok(await structure.SetUnitActiveAsync(id, body.IsActive, cancellationToken).ConfigureAwait(false));

    [HttpGet("legal-entities")]
    public async Task<IActionResult> ListLegalEntities(CancellationToken cancellationToken)
    {
        var scope = await RequireScopeAsync(cancellationToken).ConfigureAwait(false);
        if (scope is null) return ScopeMissing();
        return Ok(await legalEntityProfiles.ListAsync(scope.Value.OrgId, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("legal-entities")]
    public async Task<IActionResult> CreateLegalEntity([FromBody] LegalEntityBody body, CancellationToken cancellationToken)
    {
        var scope = await RequireScopeAsync(cancellationToken).ConfigureAwait(false);
        if (scope is null) return ScopeMissing();
        var created = await structure.CreateLegalEntityAsync(
            new CreateLegalEntityInput(scope.Value.OrgId, body.Code ?? string.Empty, body.Name ?? string.Empty, body.TaxId),
            cancellationToken).ConfigureAwait(false);
        await legalEntityProfiles.SaveProfileAsync(created.Id, body.Profile, cancellationToken).ConfigureAwait(false);
        return Ok(await legalEntityProfiles.GetAsync(created.Id, cancellationToken).ConfigureAwait(false));
    }

    [HttpPut("legal-entities/{id:guid}")]
    public async Task<IActionResult> UpdateLegalEntity(Guid id, [FromBody] LegalEntityBody body, CancellationToken cancellationToken)
    {
        await structure.UpdateLegalEntityAsync(
            id,
            new UpdateLegalEntityInput(body.Code ?? string.Empty, body.Name ?? string.Empty, body.TaxId),
            cancellationToken).ConfigureAwait(false);
        await legalEntityProfiles.SaveProfileAsync(id, body.Profile, cancellationToken).ConfigureAwait(false);
        return Ok(await legalEntityProfiles.GetAsync(id, cancellationToken).ConfigureAwait(false));
    }

    [HttpPatch("legal-entities/{id:guid}/active")]
    public async Task<IActionResult> SetLegalEntityActive(Guid id, [FromBody] ActiveBody body, CancellationToken cancellationToken)
    {
        await structure.SetLegalEntityActiveAsync(id, body.IsActive, cancellationToken).ConfigureAwait(false);
        return Ok(await legalEntityProfiles.GetAsync(id, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("job-positions")]
    public async Task<IActionResult> ListJobPositions(CancellationToken cancellationToken)
    {
        var scope = await RequireScopeAsync(cancellationToken).ConfigureAwait(false);
        if (scope is null) return ScopeMissing();
        return Ok(await jobPositions.ListAsync(scope.Value.OrgId, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("job-grades")]
    public async Task<IActionResult> ListJobGrades(CancellationToken cancellationToken)
    {
        var scope = await RequireScopeAsync(cancellationToken).ConfigureAwait(false);
        if (scope is null) return ScopeMissing();
        return Ok(await jobGrades.ListAsync(scope.Value.OrgId, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("job-titles")]
    public async Task<IActionResult> ListJobTitles(CancellationToken cancellationToken)
    {
        var scope = await RequireScopeAsync(cancellationToken).ConfigureAwait(false);
        if (scope is null) return ScopeMissing();
        return Ok(await jobTitleCatalog.ListAsync(scope.Value.OrgId, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("job-titles")]
    public async Task<IActionResult> CreateJobTitle([FromBody] NamedBody body, CancellationToken cancellationToken)
    {
        var scope = await RequireScopeAsync(cancellationToken).ConfigureAwait(false);
        if (scope is null) return ScopeMissing();
        return Ok(await structure.CreateJobTitleAsync(
            new CreateJobTitleInput(scope.Value.OrgId, body.Code ?? string.Empty, body.Name ?? string.Empty),
            cancellationToken).ConfigureAwait(false));
    }

    [HttpPut("job-titles/{id:guid}")]
    public async Task<IActionResult> UpdateJobTitle(Guid id, [FromBody] NamedBody body, CancellationToken cancellationToken) =>
        Ok(await structure.UpdateJobTitleAsync(
            id,
            new UpdateJobTitleInput(body.Code ?? string.Empty, body.Name ?? string.Empty),
            cancellationToken).ConfigureAwait(false));

    [HttpGet("positions")]
    public async Task<IActionResult> ListPositions(CancellationToken cancellationToken)
    {
        var scope = await RequireScopeAsync(cancellationToken).ConfigureAwait(false);
        if (scope is null) return ScopeMissing();
        return Ok(await structure.ListPositionsAsync(scope.Value.OrgId, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("positions")]
    public async Task<IActionResult> CreatePosition([FromBody] PositionBody body, CancellationToken cancellationToken)
    {
        var scope = await RequireScopeAsync(cancellationToken).ConfigureAwait(false);
        if (scope is null) return ScopeMissing();
        return Ok(await structure.CreatePositionAsync(new CreatePositionInput(
            scope.Value.OrgId,
            body.OrganizationalUnitId,
            body.JobTitleId,
            body.ReportsToPositionId), cancellationToken).ConfigureAwait(false));
    }

    [HttpPut("positions/{id:guid}")]
    public async Task<IActionResult> UpdatePosition(Guid id, [FromBody] PositionUpdateBody body, CancellationToken cancellationToken) =>
        Ok(await structure.UpdatePositionAsync(
            id,
            new UpdatePositionInput(body.JobTitleId, body.ReportsToPositionId),
            cancellationToken).ConfigureAwait(false));

    [HttpPatch("positions/{id:guid}/active")]
    public async Task<IActionResult> SetPositionActive(Guid id, [FromBody] ActiveBody body, CancellationToken cancellationToken) =>
        Ok(await structure.SetPositionActiveAsync(id, body.IsActive, cancellationToken).ConfigureAwait(false));

    [HttpGet("templates/{templateCode}/preview")]
    public IActionResult Preview(string templateCode, [FromQuery] string rootName) =>
        Ok(structure.PreviewTemplate(templateCode, rootName));

    [HttpPost("templates/apply")]
    public async Task<IActionResult> Apply([FromBody] ApplyTemplateBody body, CancellationToken cancellationToken)
    {
        var scope = await RequireScopeAsync(cancellationToken).ConfigureAwait(false);
        if (scope is null) return ScopeMissing();
        var nodes = (body.Nodes ?? []).Select(node => new OrgStructureTemplateNode(
            node.Code,
            node.Name,
            node.UnitType,
            node.ParentCode,
            node.Path)).ToArray();
        var written = await structure.ApplyTemplateAsync(new ApplyOrgStructureTemplateInput(
            scope.Value.OrgId,
            scope.Value.TenantId,
            body.TemplateCode ?? string.Empty,
            body.RootName ?? string.Empty,
            body.Confirmed,
            nodes), cancellationToken).ConfigureAwait(false);
        return Ok(written);
    }

    [HttpGet("units/member-summary")]
    public async Task<IActionResult> UnitMemberSummary(CancellationToken cancellationToken)
    {
        var scope = await RequireScopeAsync(cancellationToken).ConfigureAwait(false);
        if (scope is null) return ScopeMissing();
        return Ok(await unitMembers.CountByOrgAsync(scope.Value.OrgId, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("units/{unitId:guid}/members")]
    public async Task<IActionResult> ListUnitMembers(Guid unitId, CancellationToken cancellationToken)
    {
        var scope = await RequireScopeAsync(cancellationToken).ConfigureAwait(false);
        if (scope is null) return ScopeMissing();
        try
        {
            return Ok(await unitMembers.ListByUnitAsync(scope.Value.OrgId, unitId, cancellationToken)
                .ConfigureAwait(false));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("units/{unitId:guid}/members")]
    public async Task<IActionResult> CreateUnitMember(
        Guid unitId,
        [FromBody] UnitMemberBody body,
        CancellationToken cancellationToken)
    {
        var scope = await RequireScopeAsync(cancellationToken).ConfigureAwait(false);
        if (scope is null) return ScopeMissing();
        if (body.UserId == Guid.Empty || body.JobPositionId == Guid.Empty || body.JobGradeId == Guid.Empty)
            return BadRequest(new { message = "Thiếu user, vị trí công việc hoặc cấp chuyên môn." });

        try
        {
            var created = await unitMembers.CreateAsync(
                scope.Value.OrgId,
                unitId,
                new OrgUnitMemberCreateInput(
                    body.UserId,
                    body.JobPositionId,
                    body.JobGradeId,
                    body.JobTitleId),
                cancellationToken).ConfigureAwait(false);
            await SyncUserOrgClaimsAsync(
                    body.UserId,
                    unitId,
                    scope.Value.OrgId,
                    body.JobPositionId,
                    cancellationToken)
                .ConfigureAwait(false);
            return Ok(created);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("unit-members/{id:guid}")]
    public async Task<IActionResult> UpdateUnitMember(
        Guid id,
        [FromBody] UnitMemberUpdateBody body,
        CancellationToken cancellationToken)
    {
        if (body.JobPositionId == Guid.Empty || body.JobGradeId == Guid.Empty)
            return BadRequest(new { message = "Thiếu vị trí công việc hoặc cấp chuyên môn." });

        try
        {
            var updated = await unitMembers.UpdateAsync(
                id,
                new OrgUnitMemberUpdateInput(
                    body.JobPositionId,
                    body.JobGradeId,
                    body.JobTitleId),
                cancellationToken).ConfigureAwait(false);
            var scope = await RequireScopeAsync(cancellationToken).ConfigureAwait(false);
            if (scope is not null)
            {
                await SyncUserOrgClaimsAsync(
                        updated.UserId,
                        updated.OrganizationalUnitId,
                        scope.Value.OrgId,
                        updated.JobPositionId,
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("unit-members/{id:guid}/active")]
    public async Task<IActionResult> SetUnitMemberActive(
        Guid id,
        [FromBody] ActiveBody body,
        CancellationToken cancellationToken) =>
        Ok(await unitMembers.SetActiveAsync(id, body.IsActive, cancellationToken).ConfigureAwait(false));

    [HttpPut("users/{userId:guid}/scope")]
    public async Task<IActionResult> SetUserScope(
        Guid userId,
        [FromBody] UserScopeBody body,
        CancellationToken cancellationToken)
    {
        var scope = await RequireScopeAsync(cancellationToken).ConfigureAwait(false);
        if (scope is null) return ScopeMissing();

        var user = await realmUsers.FindByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (user is null) return NotFound();

        string? unitType = null;
        string? path = null;
        string? legalEntityId = null;
        Guid? organizationalUnitId = null;
        if (body.OrganizationalUnitId is Guid unitId && unitId != Guid.Empty)
        {
            var units = await structure.ListUnitsAsync(
                new ListOrganizationalUnitsQuery(scope.Value.OrgId, null, null, null),
                cancellationToken).ConfigureAwait(false);
            var unit = units.FirstOrDefault(item => item.Id == unitId);
            if (unit is null) return NotFound();
            unitType = unit.UnitType;
            path = unit.Path;
            legalEntityId = unit.LegalEntityId?.ToString();
            organizationalUnitId = unit.Id;
        }

        await realmUsers.SyncOrgClaimsAsync(
                user,
                organizationalUnitId ?? Guid.Empty,
                unitType ?? string.Empty,
                legalEntityId,
                path ?? string.Empty,
                Guid.Empty,
                cancellationToken)
            .ConfigureAwait(false);
        return Ok(new
        {
            user.Id,
            user.OrganizationalUnitId,
            user.UnitType,
            legalEntityId,
            path,
        });
    }

    private async Task SyncUserOrgClaimsAsync(
        Guid userId,
        Guid unitId,
        Guid orgId,
        Guid positionId,
        CancellationToken cancellationToken)
    {
        var user = await realmUsers.FindByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (user is null) return;

        var units = await structure.ListUnitsAsync(
            new ListOrganizationalUnitsQuery(orgId, null, null, null),
            cancellationToken).ConfigureAwait(false);
        var unit = units.FirstOrDefault(item => item.Id == unitId);
        if (unit is null) return;

        await realmUsers.SyncOrgClaimsAsync(
                user,
                unit.Id,
                unit.UnitType,
                unit.LegalEntityId?.ToString(),
                unit.Path,
                positionId,
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<(Guid TenantId, Guid OrgId)?> RequireScopeAsync(CancellationToken cancellationToken)
    {
        var tenantId = await currentTenant.GetIdAsync(cancellationToken).ConfigureAwait(false);
        var orgId = await currentOrg.GetIdAsync(cancellationToken).ConfigureAwait(false);
        if (tenantId is null || tenantId == Guid.Empty || orgId is null || orgId == Guid.Empty)
            return null;
        return (tenantId.Value, orgId.Value);
    }

    private ObjectResult ScopeMissing() =>
        StatusCode(StatusCodes.Status403Forbidden, new { message = "Thiếu tenant hoặc tổ chức đang làm việc." });

    public sealed class CreateUnitBody
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? UnitType { get; set; }
        public Guid? ParentId { get; set; }
        public int SortOrder { get; set; }
        public Guid? LegalEntityId { get; set; }
    }

    public sealed class UpdateUnitBody
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? UnitType { get; set; }
        public int SortOrder { get; set; }
        public Guid? LegalEntityId { get; set; }
    }

    public sealed class MoveUnitBody
    {
        public Guid? ParentId { get; set; }
    }

    public sealed class LegalEntityBody
    {
        public string? Code { get; set; }

        public string? Name { get; set; }

        public string? TaxId { get; set; }

        public LegalEntityProfilePayload? Profile { get; set; }
    }

    public sealed class UnitTypeBody
    {
        public string? Code { get; set; }

        public string? Name { get; set; }

        public string? Icon { get; set; }
    }

    public sealed class NamedBody
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
    }

    public sealed class PositionBody
    {
        public Guid OrganizationalUnitId { get; set; }
        public Guid? JobTitleId { get; set; }
        public Guid? ReportsToPositionId { get; set; }
    }

    public sealed class PositionUpdateBody
    {
        public Guid? JobTitleId { get; set; }
        public Guid? ReportsToPositionId { get; set; }
    }

    public sealed class ActiveBody
    {
        public bool IsActive { get; set; }
    }

    public sealed class UserScopeBody
    {
        public Guid? OrganizationalUnitId { get; set; }
    }

    public sealed class UnitMemberBody
    {
        public Guid UserId { get; set; }

        public Guid JobPositionId { get; set; }

        public Guid JobGradeId { get; set; }

        public Guid? JobTitleId { get; set; }
    }

    public sealed class UnitMemberUpdateBody
    {
        public Guid JobPositionId { get; set; }

        public Guid JobGradeId { get; set; }

        public Guid? JobTitleId { get; set; }
    }

    public sealed class ApplyTemplateBody
    {
        public string? TemplateCode { get; set; }
        public string? RootName { get; set; }
        public bool Confirmed { get; set; }
        public TemplateNodeBody[]? Nodes { get; set; }
    }

    public sealed class TemplateNodeBody
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string UnitType { get; set; } = string.Empty;
        public string? ParentCode { get; set; }
        public string Path { get; set; } = string.Empty;
    }
}
