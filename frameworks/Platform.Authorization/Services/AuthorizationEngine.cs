namespace Platform.Authorization.Services;

using Platform.Authorization.Abstractions;
using Platform.Authorization.Domain;

/// <summary>
/// Công cụ ủy quyền chính điều phối kiểm tra RBAC + ABAC.
/// </summary>
public sealed class AuthorizationEngine : IAuthorizationEngine
{
    private readonly IPermissionProvider _permissionProvider;
    private readonly IPolicyProvider _policyProvider;
    private readonly IPolicyEvaluator _policyEvaluator;

    public AuthorizationEngine(
        IPermissionProvider permissionProvider,
        IPolicyProvider policyProvider,
        IPolicyEvaluator policyEvaluator)
    {
        _permissionProvider = permissionProvider ?? throw new ArgumentNullException(nameof(permissionProvider));
        _policyProvider = policyProvider ?? throw new ArgumentNullException(nameof(policyProvider));
        _policyEvaluator = policyEvaluator ?? throw new ArgumentNullException(nameof(policyEvaluator));
    }

    public async Task<bool> HasPermissionAsync(
        Guid userId,
        string permission,
        CancellationToken cancellationToken = default)
    {
        return await _permissionProvider.HasPermissionAsync(userId, permission, cancellationToken);
    }

    public async Task<AccessDecision> EvaluatePoliciesAsync(
        Guid userId,
        string permission,
        object resource,
        CancellationToken cancellationToken = default)
    {
        var permissionsByRole = await _permissionProvider.GetPermissionsByRoleAsync(userId, cancellationToken);
        var grantingRoles = permissionsByRole
            .Where(r => PermissionMatcher.Grants(r.Value, permission))
            .ToList();

        if (grantingRoles.Count == 0)
        {
            return AccessDecision.Deny("Người dùng không có quyền hạn yêu cầu");
        }

        // ADR Q7: wildcard bỏ qua ABAC (phạm vi của wildcard TenantAdmin/OrgAdmin vẫn là một câu hỏi mở).
        if (grantingRoles.Any(r => PermissionMatcher.HasWildcard(r.Value)))
        {
            return AccessDecision.Allow(policyCode: "System.All");
        }

        // ADR D11/Q8: các chính sách được AND'ed trong một vai trò; các vai trò cấp quyền được OR'ed.
        // Các vai trò không cấp quyền không đóng góp đường dẫn.
        var anyPolicyAssigned = false;
        string? unresolvedReason = null;
        foreach (var (roleId, _) in grantingRoles)
        {
            List<Policy> policies;
            try
            {
                policies = (await _policyProvider.GetPoliciesByRoleAsync(roleId, cancellationToken)).ToList();
            }
            catch (UnresolvedPolicyException ex)
            {
                // Vai trò được cấu hình sai: đường dẫn của nó không thành công; các vai trò khác vẫn có thể vượt qua.
                unresolvedReason = ex.Message;
                continue;
            }

            if (policies.Count == 0)
            {
                continue;
            }

            anyPolicyAssigned = true;
            if (await AllPoliciesPassAsync(userId, policies, resource, cancellationToken))
            {
                return AccessDecision.Allow(string.Join("+", policies.Select(p => p.Code)));
            }
        }

        if (unresolvedReason != null)
        {
            return AccessDecision.Deny(unresolvedReason);
        }

        return anyPolicyAssigned
            ? AccessDecision.Deny("Không có đường dẫn vai trò nào thỏa mãn tất cả các chính sách của nó")
            : AccessDecision.Deny("Không có chính sách được gán cho người dùng");
    }

    private async Task<bool> AllPoliciesPassAsync(
        Guid userId,
        IEnumerable<Policy> policies,
        object resource,
        CancellationToken cancellationToken)
    {
        foreach (var policy in policies)
        {
            if (!await _policyEvaluator.EvaluateAsync(userId, policy, resource, cancellationToken))
            {
                return false;
            }
        }

        return true;
    }

    public async Task<bool> AuthorizeAsync(
        Guid userId,
        string permission,
        object? resource = null,
        CancellationToken cancellationToken = default)
    {
        // Kiểm tra chỉ RBAC
        if (!await HasPermissionAsync(userId, permission, cancellationToken))
        {
            return false;
        }

        // Nếu không có tài nguyên, chỉ RBAC là đủ
        if (resource == null)
        {
            return true;
        }

        // Đánh giá ABAC
        var result = await EvaluatePoliciesAsync(userId, permission, resource, cancellationToken);
        return result.IsAllowed;
    }
}
