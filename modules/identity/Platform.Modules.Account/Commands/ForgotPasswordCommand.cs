namespace Platform.Modules.Account.Commands;

using Platform.DDD.Domain.Shared.Messaging;

/// <summary>
/// Yêu cầu reset password qua email. Không tiết lộ email có tồn tại hay không (anti-enumeration).
/// </summary>
public sealed record ForgotPasswordCommand : ICommand
{
    public required string Email { get; init; }
}
