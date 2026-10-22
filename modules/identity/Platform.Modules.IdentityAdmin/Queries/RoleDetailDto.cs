namespace Platform.Modules.IdentityAdmin.Queries;

public sealed record RoleDetailDto
{
    public required Guid RoleId { get; init; }
    public required string Name { get; init; }
    public string? DisplayName { get; init; }
    public required int RoleLevel { get; init; }
    public required bool IsSystemRole { get; init; }
    public required bool IsActive { get; init; }
    public required IReadOnlyCollection<string> Permissions { get; init; }
}
