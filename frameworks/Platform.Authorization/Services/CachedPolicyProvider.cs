namespace Platform.Authorization.Services;

using Microsoft.Extensions.Caching.Memory;
using Platform.Authorization.Abstractions;
using Platform.Authorization.Domain;

/// <summary>
/// Decorator cho PolicyProvider với hỗ trợ caching.
/// Caches chính sách theo code với việc làm mất hiệu lực dựa trên TTL.
/// </summary>
public sealed class CachedPolicyProvider : IPolicyProvider
{
    private readonly IPolicyProvider _inner;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheDuration;

    private const string PolicyCacheKeyPrefix = "policy:";

    public CachedPolicyProvider(
        IPolicyProvider inner,
        IMemoryCache cache,
        TimeSpan? cacheDuration = null)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _cacheDuration = cacheDuration ?? TimeSpan.FromHours(1);
    }

    public async Task<Policy?> GetPolicyByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{PolicyCacheKeyPrefix}{code}";

        if (_cache.TryGetValue(cacheKey, out Policy? policy))
        {
            return policy;
        }

        policy = await _inner.GetPolicyByCodeAsync(code, cancellationToken);

        if (policy != null)
        {
            _cache.Set(cacheKey, policy, _cacheDuration);
        }

        return policy;
    }

    public async Task<IEnumerable<Policy>> GetPoliciesByRoleAsync(
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        return await _inner.GetPoliciesByRoleAsync(roleId, cancellationToken);
    }

    public async Task<IEnumerable<Policy>> GetPoliciesByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // User policies are not cached (depends on role assignment which can change)
        // Caller should invalidate user authorization snapshot when roles change
        return await _inner.GetPoliciesByUserAsync(userId, cancellationToken);
    }

    public async Task<Policy?> GetPolicyByIdAsync(
        Guid policyId,
        CancellationToken cancellationToken = default)
    {
        return await _inner.GetPolicyByIdAsync(policyId, cancellationToken);
    }

    public async Task<IEnumerable<Policy>> GetSystemPoliciesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _inner.GetSystemPoliciesAsync(cancellationToken);
    }

    public async Task InvalidateCacheAsync(CancellationToken cancellationToken = default)
    {
        ClearPolicyCaches();
        if (_inner is CachedPolicyProvider cachedInner)
        {
            await cachedInner.InvalidateCacheAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Invalidate cached policy by code.
    /// Call this when a policy is updated or deleted.
    /// </summary>
    public void InvalidatePolicyCache(string code)
    {
        var cacheKey = $"{PolicyCacheKeyPrefix}{code}";
        _cache.Remove(cacheKey);
    }

    /// <summary>
    /// Clear all policy caches.
    /// </summary>
    public void ClearPolicyCaches()
    {
        // MemoryCache doesn't provide enumeration, so we'd need to track keys separately
        // For now, this is a placeholder for distributed cache scenarios
    }
}
