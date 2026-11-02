namespace Platform.Modules.IdentityAdmin.Abstractions;

public sealed record PlatformUserListItem(
    Guid Id,
    string Username,
    string? Email,
    string? DisplayName,
    string? PhoneNumber,
    bool IsLocked,
    IReadOnlyList<string> RoleNames);

public sealed class CreatePlatformUserRequest
{
    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? DisplayName { get; set; }

    public IReadOnlyCollection<Guid>? RoleIds { get; set; }
}

public sealed class UpdatePlatformUserRequest
{
    public string? Email { get; set; }

    public string? DisplayName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Password { get; set; }

    public bool? IsLocked { get; set; }

    public IReadOnlyCollection<Guid>? RoleIds { get; set; }
}

public interface IPlatformUserAdminService
{
    Task<IReadOnlyList<PlatformUserListItem>> ListAsync(
        string? search,
        CancellationToken cancellationToken = default);

    Task<PlatformUserListItem?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<PlatformUserListItem> CreateAsync(
        CreatePlatformUserRequest request,
        CancellationToken cancellationToken = default);

    Task<PlatformUserListItem> UpdateAsync(
        Guid id,
        UpdatePlatformUserRequest request,
        CancellationToken cancellationToken = default);

    Task<PlatformUserListItem> SetLockedAsync(
        Guid id,
        bool isLocked,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
