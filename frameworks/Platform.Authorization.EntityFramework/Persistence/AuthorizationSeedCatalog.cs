namespace Platform.Authorization.EntityFramework.Persistence;

using Platform.Authentication.Identity.Domain;
using Platform.Authorization.Domain;

/// <summary>
/// Tập role / policy cần seed (ADR V5). Platform chỉ đóng góp role quản trị và policy chung
/// (<see cref="AuthorizationSeeder.AddPlatformDefaults"/>); sản phẩm thêm role / policy nghiệp vụ
/// thông qua <c>AddPlatformAuthorizationSeed(...)</c>.
/// </summary>
public sealed class AuthorizationSeedCatalog
{
    private readonly Dictionary<string, Role> _roles = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Policy> _policies = new(StringComparer.Ordinal);

    public IReadOnlyCollection<Role> Roles => _roles.Values;

    public IReadOnlyCollection<Policy> Policies => _policies.Values;

    public AuthorizationSeedCatalog AddRole(Role role)
    {
        ArgumentNullException.ThrowIfNull(role);
        if (string.IsNullOrWhiteSpace(role.Name))
            throw new ArgumentException("Role seed phải có Name.", nameof(role));

        role.NormalizedName ??= role.Name.ToUpperInvariant();
        if (!_roles.TryAdd(role.NormalizedName, role))
            throw new InvalidOperationException($"Role '{role.Name}' đã được khai báo seed.");
        return this;
    }

    public AuthorizationSeedCatalog AddPolicy(Policy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);
        if (string.IsNullOrWhiteSpace(policy.Code))
            throw new ArgumentException("Policy seed phải có Code.", nameof(policy));

        if (!_policies.TryAdd(policy.Code, policy))
            throw new InvalidOperationException($"Policy '{policy.Code}' đã được khai báo seed.");
        return this;
    }
}

/// <summary>Bộ đánh dấu DI cho một đóng góp seed lúc khởi động.</summary>
public sealed class AuthorizationSeedContribution(Action<AuthorizationSeedCatalog> configure)
{
    internal Action<AuthorizationSeedCatalog> Configure { get; } = configure ?? throw new ArgumentNullException(nameof(configure));
}
