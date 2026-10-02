namespace Platform.Modules.Account.Commands;

using Platform.DDD.Domain.Shared.Messaging;

/// <summary>
/// Logout command. Hệ thống dùng JWT stateless, chưa có hạ tầng revoke/blacklist token
/// server-side (nợ kỹ thuật) — handler chỉ xác thực actor đang đăng nhập hợp lệ.
/// </summary>
public sealed record LogoutCommand : ICommand;
