namespace Platform.Modules.Account.Commands;

using Platform.DDD.Domain.Shared.Messaging;

/// <summary>
/// Đổi password tự phục vụ: actor đang đăng nhập đổi password của chính mình.
/// </summary>
public sealed record ChangePasswordCommand : ICommand
{
    public required string CurrentPassword { get; init; }
    public required string NewPassword { get; init; }
}
