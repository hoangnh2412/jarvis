namespace Platform.Modules.IdentityAdmin.Api.Models;

public sealed record IdentityUserCreateWrite
{
    public required string Username { get; init; }

    public required string Email { get; init; }

    public required string Password { get; init; }

    public string? PhoneNumber { get; init; }

    public string? DisplayName { get; init; }

    public required IReadOnlyCollection<Guid> RoleIds { get; init; }
}

public sealed record IdentityUserUpdateWrite
{
    public required string Username { get; init; }

    public required string Email { get; init; }

    public string? PhoneNumber { get; init; }

    public string? DisplayName { get; init; }

    public string? Password { get; init; }

    public required bool IsLocked { get; init; }

    public required IReadOnlyCollection<Guid> RoleIds { get; init; }
}

public sealed record IdentityUserLockWrite
{
    public required bool IsLocked { get; init; }
}

public sealed record IdentityUserScopeWrite
{
    public Guid? OrganizationalUnitId { get; init; }
}

public sealed record IdentityRoleWrite
{
    public required string Name { get; init; }

    public string? DisplayName { get; init; }

    /// <summary>Vai trò tạo từ form admin dùng mức 100, thấp hơn tenant admin đã seed.</summary>
    public int RoleLevel { get; init; } = 100;

    public bool IsActive { get; init; } = true;
}

public sealed record IdentityRolePermissionsWrite
{
    public required IReadOnlyCollection<string> Permissions { get; init; }
}

public sealed record IdentityRolePoliciesWrite
{
    public required IReadOnlyCollection<string> Policies { get; init; }
}
