using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Platform.Authentication;

namespace Sample.Authentication;

/// <summary>Scope password policy — tenant từ header (giống credential auth).</summary>
public sealed class SamplePasswordPolicyScopeProvider(
    IHttpContextAccessor httpContextAccessor,
    IConfiguration configuration) : IPasswordPolicyScopeProvider
{
    private readonly string _tenantHeaderKey = configuration.GetValue<string>("TenantHeaderKey") ?? "X-Tenant-Id";

    public Task<PasswordPolicyScope> GetScopeAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = TenantAuthContext.TryGetTenantId(httpContextAccessor.HttpContext, _tenantHeaderKey)
            ?? SampleAuthTenants.DevTenantId;

        return Task.FromResult(new PasswordPolicyScope { TenantId = tenantId });
    }
}
