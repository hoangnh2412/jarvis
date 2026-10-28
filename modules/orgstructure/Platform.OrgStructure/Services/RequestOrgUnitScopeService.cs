using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Platform.DDD.Domain.Shared.ExceptionHandling;
using Platform.OrgStructure.Models;

namespace Platform.OrgStructure.Services;

public sealed class RequestOrgUnitScopeService(
    IOptions<OrgStructureOptions> options,
    IHttpContextAccessor httpContextAccessor,
    IOrgStructureManager structure) : IRequestOrgUnitScope
{
    public bool FilterEnabled => options.Value.FilterABACByOrgStructure;

    public async Task<RequestOrgUnitScope?> ResolveAsync(Guid? orgId, CancellationToken cancellationToken = default)
    {
        if (!FilterEnabled)
            return null;

        if (orgId is not Guid currentOrg || currentOrg == Guid.Empty)
            throw new BadRequestException(OrgStructureErrorCode.InvalidInput, "Thiếu phạm vi tổ chức.");

        var headers = httpContextAccessor.HttpContext?.Request.Headers;
        var unitIdRaw = headers?[OrgStructureHeaders.UnitId].ToString();
        var unitTypeRaw = headers?[OrgStructureHeaders.UnitType].ToString();
        if (string.IsNullOrWhiteSpace(unitIdRaw) || string.IsNullOrWhiteSpace(unitTypeRaw))
            throw new BadRequestException(OrgStructureErrorCode.InvalidInput, "Thiếu X-Org-Unit-Id hoặc X-Org-Unit-Type.");

        if (!Guid.TryParse(unitIdRaw.Trim(), out var unitId) || unitId == Guid.Empty)
            throw new BadRequestException(OrgStructureErrorCode.InvalidInput, "X-Org-Unit-Id không hợp lệ.");

        OrganizationalUnitDescriptor unit;
        try
        {
            unit = await structure.GetUnitAsync(unitId, cancellationToken).ConfigureAwait(false);
        }
        catch (NotFoundException)
        {
            throw new BadRequestException(OrgStructureErrorCode.InvalidInput, "Đơn vị không thuộc tổ chức hiện tại.");
        }

        if (unit.OrgId != currentOrg)
            throw new BadRequestException(OrgStructureErrorCode.InvalidInput, "Đơn vị không thuộc tổ chức hiện tại.");

        var unitType = unitTypeRaw.Trim();
        if (!string.Equals(unit.UnitType, unitType, StringComparison.Ordinal))
            throw new BadRequestException(OrgStructureErrorCode.InvalidInput, "X-Org-Unit-Type không khớp đơn vị.");

        return new RequestOrgUnitScope
        {
            OrganizationalUnitId = unit.Id,
            UnitType = unit.UnitType,
            Path = unit.Path,
        };
    }
}
