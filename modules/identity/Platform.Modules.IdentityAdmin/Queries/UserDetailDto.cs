namespace Platform.Modules.IdentityAdmin.Queries;

public sealed record UserDetailDto
{
    public required Guid UserId { get; init; }
    public required string Username { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string? DisplayName { get; init; }
    public required bool IsLocked { get; init; }
    public required IReadOnlyCollection<Guid> RoleIds { get; init; }
    public required IReadOnlyCollection<string> Roles { get; init; }
    public Guid? TenantId { get; init; }
    public Guid? OrgId { get; init; }
}
