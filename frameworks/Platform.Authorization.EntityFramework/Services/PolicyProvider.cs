namespace Platform.Authorization.EntityFramework.Services;

using Microsoft.EntityFrameworkCore;
using Platform.Authorization.Abstractions;
using Platform.Authorization.Domain;
using Platform.Authorization.EntityFramework.Persistence;

/// <summary>
/// Cung cấp các chính sách cho đánh giá ABAC.
/// Truy vấn Policy và PolicyRule từ DbContext.
/// </summary>
public sealed class PolicyProvider : IPolicyProvider
{
    private readonly IdentityDbContextBase _context;

    public PolicyProvider(IdentityDbContextBase context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<Policy>> GetPoliciesByRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        // Truy vấn RoleClaims nơi ClaimType = "Policy" và trích xuất mã chính sách
        var policyCodes = await _context.RoleClaims
            .Where(rc => rc.RoleId == roleId && rc.ClaimType == "Policy")
            .Select(rc => rc.ClaimValue ?? string.Empty)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (!policyCodes.Any())
        {
            return Enumerable.Empty<Policy>();
        }

        // Truy vấn Policies với Rules được bao gồm
        var policies = await _context.Policies
            .Include(p => p.Rules)
            .Where(p => policyCodes.Contains(p.Code))
            .ToListAsync(cancellationToken);

        var missingCodes = policyCodes.Except(policies.Select(p => p.Code)).ToList();
        if (missingCodes.Count > 0)
        {
            throw new UnresolvedPolicyException(roleId, missingCodes);
        }

        return policies;
    }

    public async Task<IEnumerable<Policy>> GetPoliciesByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Truy vấn các vai trò của người dùng
        var roleIds = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (!roleIds.Any())
        {
            return Enumerable.Empty<Policy>();
        }

        // Query RoleClaims for all user's roles, extract unique policy codes
        var policyCodes = await _context.RoleClaims
            .Where(rc => roleIds.Contains(rc.RoleId) && rc.ClaimType == "Policy")
            .Select(rc => rc.ClaimValue ?? string.Empty)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (!policyCodes.Any())
        {
            return Enumerable.Empty<Policy>();
        }

        // Query Policies with Rules included
        var policies = await _context.Policies
            .Include(p => p.Rules)
            .Where(p => policyCodes.Contains(p.Code))
            .ToListAsync(cancellationToken);

        return policies;
    }

    public async Task<Policy?> GetPolicyByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Policies
            .Include(p => p.Rules)
            .FirstOrDefaultAsync(p => p.Code == code, cancellationToken);
    }

    public async Task<Policy?> GetPolicyByIdAsync(Guid policyId, CancellationToken cancellationToken = default)
    {
        return await _context.Policies
            .Include(p => p.Rules)
            .FirstOrDefaultAsync(p => p.Id == policyId, cancellationToken);
    }

    public async Task<IEnumerable<Policy>> GetSystemPoliciesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Policies
            .Include(p => p.Rules)
            .Where(p => p.IsSystemPolicy)
            .ToListAsync(cancellationToken);
    }

    public async Task InvalidateCacheAsync(CancellationToken cancellationToken = default)
    {
        // No-op in base implementation; overridden in CachedPolicyProvider (Phase 7)
        await Task.CompletedTask;
    }
}
