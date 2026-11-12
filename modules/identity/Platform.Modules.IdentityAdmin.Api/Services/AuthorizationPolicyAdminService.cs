using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Platform.Authentication;
using Platform.Authorization.Abstractions;
using Platform.Authorization.Domain;
using Platform.Authorization.EntityFramework.Persistence;
using Platform.Authorization.Operands;
using Platform.Authorization.Services;
using Platform.DDD.Domain.Services;
using Platform.Modules.IdentityAdmin.Abstractions;

namespace Platform.Modules.IdentityAdmin.Api.Services;

public sealed class AuthorizationPolicyAdminService : IAuthorizationPolicyAdminService
{
    private static readonly Regex PolicyCodePattern = new(@"^[A-Z][A-Z0-9_]*$", RegexOptions.CultureInvariant);

    private readonly IdentityDbContextBase _db;
    private readonly OperandValidator _operandValidator;
    private readonly IOperandRegistry _operandRegistry;
    private readonly IPermissionProvider _permissionProvider;
    private readonly ICurrentUser<CurrentUserInfo> _currentUser;
    private readonly IPermissionCatalogProvider? _catalogProvider;

    public AuthorizationPolicyAdminService(
        IdentityDbContextBase db,
        OperandValidator operandValidator,
        IOperandRegistry operandRegistry,
        IPermissionProvider permissionProvider,
        ICurrentUser<CurrentUserInfo> currentUser,
        IPermissionCatalogProvider? catalogProvider = null)
    {
        _db = db;
        _operandValidator = operandValidator;
        _operandRegistry = operandRegistry;
        _permissionProvider = permissionProvider;
        _currentUser = currentUser;
        _catalogProvider = catalogProvider;
    }

    public IReadOnlyList<AuthorizationOperandDto> ListOperands() =>
        _operandRegistry.All
            .OrderBy(o => o.Path, StringComparer.Ordinal)
            .Select(o => new AuthorizationOperandDto(o.Path, o.Source.ToString(), o.Kind.ToString(), o.ValueType.ToString()))
            .ToList();

    public IReadOnlyList<string> ListOperators() =>
        OperandValidator.GetWhitelistedOperators().OrderBy(o => o, StringComparer.Ordinal).ToList();

    public async Task<IReadOnlyList<AuthorizationPolicyDto>> ListPoliciesAsync(CancellationToken cancellationToken = default)
    {
        var policies = await _db.Policies
            .AsNoTracking()
            .Include(p => p.Rules)
            .OrderBy(p => p.Code)
            .ToListAsync(cancellationToken);

        var assignedCodes = await LoadAssignedPolicyCodesAsync(cancellationToken);
        return policies.Select(p => MapPolicy(p, assignedCodes.Contains(p.Code))).ToList();
    }

    public async Task<AuthorizationPolicyDto> GetPolicyAsync(Guid policyId, CancellationToken cancellationToken = default)
    {
        var policy = await _db.Policies
            .AsNoTracking()
            .Include(p => p.Rules)
            .FirstOrDefaultAsync(p => p.Id == policyId, cancellationToken)
            ?? throw new InvalidOperationException("Không tìm thấy policy.");

        var inUse = await IsPolicyAssignedAsync(policy.Code, cancellationToken);
        return MapPolicy(policy, inUse);
    }

    public async Task<AuthorizationPolicyDto> CreatePolicyAsync(
        AuthorizationPolicyWriteDto input,
        CancellationToken cancellationToken = default)
    {
        await EnsureCanManagePoliciesAsync(cancellationToken);

        var code = NormalizeCode(input.Code);
        ValidateCode(code);

        if (await _db.Policies.AnyAsync(p => p.Code == code, cancellationToken))
            throw new InvalidOperationException($"Policy '{code}' đã tồn tại.");

        var rules = ValidateRules(input.Rules);
        var now = DateTime.UtcNow;
        var policy = new Policy
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = NormalizeName(input.Name),
            ResourceType = string.IsNullOrWhiteSpace(input.ResourceType) ? null : input.ResourceType.Trim(),
            Action = string.IsNullOrWhiteSpace(input.Action) ? null : input.Action.Trim(),
            IsSystemPolicy = false,
            CreatedAt = now,
            UpdatedAt = now,
            Rules = rules.Select(r => new PolicyRule
            {
                Id = Guid.NewGuid(),
                Order = r.Order,
                LeftOperand = r.LeftOperand,
                Operator = r.Operator,
                RightOperand = r.RightOperand,
                CreatedAt = now,
            }).ToList(),
        };

        foreach (var rule in policy.Rules)
            rule.PolicyId = policy.Id;

        _db.Policies.Add(policy);
        await _db.SaveChangesAsync(cancellationToken);
        return MapPolicy(policy, false);
    }

    public async Task<AuthorizationPolicyDto> UpdatePolicyAsync(
        Guid policyId,
        AuthorizationPolicyWriteDto input,
        CancellationToken cancellationToken = default)
    {
        await EnsureCanManagePoliciesAsync(cancellationToken);

        var policy = await _db.Policies
            .Include(p => p.Rules)
            .FirstOrDefaultAsync(p => p.Id == policyId, cancellationToken)
            ?? throw new InvalidOperationException("Không tìm thấy policy.");

        if (policy.IsSystemPolicy)
            throw new InvalidOperationException("Không thể sửa policy hệ thống.");

        if (await IsPolicyAssignedAsync(policy.Code, cancellationToken))
            throw new InvalidOperationException("Không thể sửa policy đang được gán cho vai trò — gỡ gán trước khi sửa.");

        var code = NormalizeCode(input.Code);
        ValidateCode(code);
        if (!string.Equals(policy.Code, code, StringComparison.Ordinal)
            && await _db.Policies.AnyAsync(p => p.Code == code, cancellationToken))
        {
            throw new InvalidOperationException($"Policy '{code}' đã tồn tại.");
        }

        var rules = ValidateRules(input.Rules);
        var now = DateTime.UtcNow;

        if (!string.Equals(policy.Code, code, StringComparison.Ordinal))
            policy.Code = code;

        policy.Name = NormalizeName(input.Name);
        policy.ResourceType = string.IsNullOrWhiteSpace(input.ResourceType) ? null : input.ResourceType.Trim();
        policy.Action = string.IsNullOrWhiteSpace(input.Action) ? null : input.Action.Trim();
        policy.UpdatedAt = now;

        _db.PolicyRules.RemoveRange(policy.Rules);
        policy.Rules.Clear();
        foreach (var rule in rules)
        {
            policy.Rules.Add(new PolicyRule
            {
                Id = Guid.NewGuid(),
                PolicyId = policy.Id,
                Order = rule.Order,
                LeftOperand = rule.LeftOperand,
                Operator = rule.Operator,
                RightOperand = rule.RightOperand,
                CreatedAt = now,
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        return MapPolicy(policy, false);
    }

    public async Task DeletePolicyAsync(Guid policyId, CancellationToken cancellationToken = default)
    {
        await EnsureCanManagePoliciesAsync(cancellationToken);

        var policy = await _db.Policies
            .Include(p => p.Rules)
            .FirstOrDefaultAsync(p => p.Id == policyId, cancellationToken)
            ?? throw new InvalidOperationException("Không tìm thấy policy.");

        if (policy.IsSystemPolicy)
            throw new InvalidOperationException("Không thể xóa policy hệ thống.");

        var assigned = await _db.RoleClaims.AnyAsync(
            c => c.ClaimType == "Policy" && c.ClaimValue == policy.Code,
            cancellationToken);
        if (assigned)
            throw new InvalidOperationException("Không thể xóa policy đang được gán cho vai trò — gỡ gán trước khi xóa.");

        _db.PolicyRules.RemoveRange(policy.Rules);
        _db.Policies.Remove(policy);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureCanManagePoliciesAsync(CancellationToken cancellationToken)
    {
        var actor = await _currentUser.GetAsync(cancellationToken);
        if (actor?.UserId is null)
            throw new UnauthorizedAccessException();

        var userId = actor.UserId.Value;
        if (await _permissionProvider.HasPermissionAsync(userId, "*", cancellationToken)
            || await _permissionProvider.HasPermissionAsync(userId, "System.All", cancellationToken)
            || await _permissionProvider.HasPermissionAsync(userId, "System.Role.Manage", cancellationToken))
        {
            return;
        }

        if (_catalogProvider is not null)
        {
            var granted = await _permissionProvider.GetPermissionsAsync(userId, cancellationToken);
            if (granted.Any(_catalogProvider.CanManagePolicies))
                return;
        }

        throw new UnauthorizedAccessException("Bạn không có quyền quản lý policy.");
    }

    private List<AuthorizationPolicyRuleWriteDto> ValidateRules(IReadOnlyCollection<AuthorizationPolicyRuleWriteDto>? rules)
    {
        if (rules is null || rules.Count == 0)
            throw new InvalidOperationException("Policy cần ít nhất một rule.");

        var ordered = rules
            .Select((rule, index) => new AuthorizationPolicyRuleWriteDto
            {
                Order = rule.Order > 0 ? rule.Order : index + 1,
                LeftOperand = rule.LeftOperand?.Trim() ?? string.Empty,
                Operator = rule.Operator?.Trim() ?? string.Empty,
                RightOperand = rule.RightOperand?.Trim() ?? string.Empty,
            })
            .OrderBy(r => r.Order)
            .ToList();

        foreach (var rule in ordered)
        {
            var result = _operandValidator.ValidateRule(rule.LeftOperand, rule.Operator, rule.RightOperand);
            if (!result.IsValid)
                throw new InvalidOperationException(result.ErrorMessage ?? "Rule không hợp lệ.");
        }

        return ordered;
    }

    private static void ValidateCode(string code)
    {
        if (!PolicyCodePattern.IsMatch(code))
            throw new InvalidOperationException("Mã policy phải dạng TEN_VIET_HOA (chữ HOA, số, gạch dưới).");
    }

    private static string NormalizeCode(string? code) =>
        (code ?? string.Empty).Trim().ToUpperInvariant();

    private static string NormalizeName(string? name)
    {
        var value = (name ?? string.Empty).Trim();
        if (value.Length == 0)
            throw new InvalidOperationException("Tên policy không được trống.");
        return value;
    }

    private async Task<HashSet<string>> LoadAssignedPolicyCodesAsync(CancellationToken cancellationToken)
    {
        var codes = await _db.RoleClaims
            .AsNoTracking()
            .Where(c => c.ClaimType == "Policy" && c.ClaimValue != null)
            .Select(c => c.ClaimValue!)
            .Distinct()
            .ToListAsync(cancellationToken);
        return codes.ToHashSet(StringComparer.Ordinal);
    }

    private Task<bool> IsPolicyAssignedAsync(string policyCode, CancellationToken cancellationToken) =>
        _db.RoleClaims.AnyAsync(
            c => c.ClaimType == "Policy" && c.ClaimValue == policyCode,
            cancellationToken);

    private static AuthorizationPolicyDto MapPolicy(Policy policy, bool isInUse) =>
        new(
            policy.Id,
            policy.Code,
            policy.Name,
            policy.ResourceType,
            policy.Action,
            policy.IsSystemPolicy,
            isInUse,
            policy.Rules
                .OrderBy(r => r.Order)
                .Select(r => new AuthorizationPolicyRuleDto(r.Id, r.Order, r.LeftOperand, r.Operator, r.RightOperand))
                .ToList());
}
