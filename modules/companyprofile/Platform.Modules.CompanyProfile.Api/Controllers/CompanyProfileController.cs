using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Platform.DDD.Domain.Services;
using Platform.Tenants.Services;

namespace Platform.Modules.CompanyProfile.Api.Controllers;

/// <summary>Thông tin doanh nghiệp của tenant đang làm việc: lõi platform + khối extension do sản phẩm đăng ký.</summary>
[ApiController]
[Authorize]
[Route("api/v1/app/company-profile")]
public sealed class CompanyProfileController(
    ICompanyProfileService profiles,
    ICurrentTenant<TenantCatalogProfile> currentTenant) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var owner = await RequireOwnerAsync(cancellationToken).ConfigureAwait(false);
        if (owner is null) return ScopeMissing();
        return Ok(await profiles.GetAsync(owner, cancellationToken).ConfigureAwait(false));
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] CompanyProfileSaveBody body, CancellationToken cancellationToken)
    {
        var owner = await RequireOwnerAsync(cancellationToken).ConfigureAwait(false);
        if (owner is null) return ScopeMissing();
        try
        {
            var saved = await profiles.SaveAsync(owner, body.Core ?? new CompanyProfileContent(), body.Extensions, cancellationToken)
                .ConfigureAwait(false);
            return Ok(saved);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private async Task<CompanyProfileOwner?> RequireOwnerAsync(CancellationToken cancellationToken)
    {
        var tenantId = await currentTenant.GetIdAsync(cancellationToken).ConfigureAwait(false);
        if (tenantId is null || tenantId == Guid.Empty)
            return null;
        return new CompanyProfileOwner(CompanyProfileOwnerType.Tenant, tenantId.Value);
    }

    private ObjectResult ScopeMissing() =>
        StatusCode(StatusCodes.Status403Forbidden, new { message = "Thiếu tenant đang làm việc." });

    public sealed class CompanyProfileSaveBody
    {
        public CompanyProfileContent? Core { get; set; }

        public Dictionary<string, JsonElement>? Extensions { get; set; }
    }
}
