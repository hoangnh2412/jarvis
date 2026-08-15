using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Jarvis.Authentication;
using Jarvis.DDD.Domain.Services;
using Jarvis.Multitenancy;

namespace Sample.Controllers;

/// <summary>
/// Demo ADR generic <c>ICurrentTenant&lt;T&gt;</c> + bỏ <c>IWorkContext</c>:
/// inject trực tiếp <see cref="ICurrentUser{TUser}"/> và <see cref="ICurrentTenant{TTenant}"/>.
/// </summary>
/// <remarks>
/// <para>Gửi auth (JWT/ApiKey/…) để có user profile; gửi header <c>X-Tenant-Id</c> để R2 resolve current tenant.</para>
/// <para>D5: <c>user.TenantId</c> = home; <c>tenant.GetIdAsync</c> / <c>GetAsync</c> = working (có thể khác sau <c>Change</c>).</para>
/// </remarks>
[ApiController]
[Route("api/v{version:apiVersion}/tests/current-user-tenant")]
public class CurrentUserTenantTestController : ControllerBase
{
    /// <summary>
    /// Snapshot: user profile + tenant id (R2) + tenant profile (store) — không qua facade.
    /// </summary>
    [HttpGet]
    [MapToApiVersion(2.0)]
    public async Task<IActionResult> GetContextAsync(
        [FromServices] ICurrentUser<CurrentUserInfo> currentUser,
        [FromServices] ICurrentTenant<CurrentTenantInfo> currentTenant,
        CancellationToken cancellationToken)
    {
        var user = await currentUser.GetAsync(cancellationToken).ConfigureAwait(false);
        var tenantId = await currentTenant.GetIdAsync(cancellationToken).ConfigureAwait(false);
        var tenant = await currentTenant.GetAsync(cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            User = user == null
                ? null
                : new
                {
                    user.UserId,
                    user.UserName,
                    user.TokenId,
                    HomeTenantId = user.TenantId,
                },
            Tenant = new
            {
                Id = tenantId,
                Profile = tenant == null
                    ? null
                    : new
                    {
                        tenant.TenantId,
                        tenant.Name,
                    },
            },
            HomeTenant = new
            {
                HomeTenantId = user?.TenantId,
                CurrentWorkingTenantId = tenantId,
                HomeEqualsCurrent = user?.TenantId == tenantId,
            },
        });
    }

    /// <summary>
    /// Home tenant probe: home trên user giữ nguyên khi <see cref="ICurrentTenant{TTenant}.Change"/> đổi working tenant.
    /// </summary>
    [HttpGet("home-tenant-change")]
    [MapToApiVersion(2.0)]
    public async Task<IActionResult> ProbeHomeTenantChangeAsync(
        [FromServices] ICurrentUser<CurrentUserInfo> currentUser,
        [FromServices] ICurrentTenant<CurrentTenantInfo> currentTenant,
        [FromQuery] Guid workingTenantId,
        CancellationToken cancellationToken)
    {
        if (workingTenantId == Guid.Empty)
            return BadRequest("workingTenantId is required (non-empty Guid).");

        var userBefore = await currentUser.GetAsync(cancellationToken).ConfigureAwait(false);
        var tenantIdBefore = await currentTenant.GetIdAsync(cancellationToken).ConfigureAwait(false);
        var tenantBefore = await currentTenant.GetAsync(cancellationToken).ConfigureAwait(false);

        object? during;
        using (currentTenant.Change(workingTenantId))
        {
            var userDuring = await currentUser.GetAsync(cancellationToken).ConfigureAwait(false);
            var tenantIdDuring = await currentTenant.GetIdAsync(cancellationToken).ConfigureAwait(false);
            var tenantDuring = await currentTenant.GetAsync(cancellationToken).ConfigureAwait(false);

            during = new
            {
                UserHomeTenantId = userDuring?.TenantId,
                CurrentWorkingTenantId = tenantIdDuring,
                TenantProfile = tenantDuring == null
                    ? null
                    : new { tenantDuring.TenantId, tenantDuring.Name },
                HomeUnchanged = userDuring?.TenantId == userBefore?.TenantId,
                WorkingChanged = tenantIdDuring == workingTenantId,
            };
        }

        var userAfter = await currentUser.GetAsync(cancellationToken).ConfigureAwait(false);
        var tenantIdAfter = await currentTenant.GetIdAsync(cancellationToken).ConfigureAwait(false);
        var tenantAfter = await currentTenant.GetAsync(cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            Before = new
            {
                UserHomeTenantId = userBefore?.TenantId,
                CurrentWorkingTenantId = tenantIdBefore,
                TenantProfile = tenantBefore == null
                    ? null
                    : new { tenantBefore.TenantId, tenantBefore.Name },
            },
            DuringChange = during,
            After = new
            {
                UserHomeTenantId = userAfter?.TenantId,
                CurrentWorkingTenantId = tenantIdAfter,
                TenantProfile = tenantAfter == null
                    ? null
                    : new { tenantAfter.TenantId, tenantAfter.Name },
            },
        });
    }
}
