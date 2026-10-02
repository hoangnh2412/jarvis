namespace Platform.Modules.Account.Commands;

using Platform.DDD.Domain.Shared.Messaging;

/// <summary>
/// Cập nhật profile tự phục vụ của actor đang đăng nhập. Field null = giữ nguyên.
/// </summary>
public sealed record UpdateProfileCommand : ICommand
{
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
}
