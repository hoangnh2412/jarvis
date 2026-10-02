namespace Platform.Modules.Account.Commands;

using Platform.DDD.Domain.Shared.Messaging;

/// <summary>
/// Đặt lại password bằng token nhận được từ ForgotPassword.
/// </summary>
public sealed record ResetPasswordCommand : ICommand
{
    public required string Email { get; init; }
    public required string Token { get; init; }
    public required string NewPassword { get; init; }
}
