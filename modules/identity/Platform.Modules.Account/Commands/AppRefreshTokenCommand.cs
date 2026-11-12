using Platform.DDD.Domain.Shared.Messaging;

namespace Platform.Modules.Account.Commands;

/// <summary>Đổi refresh token tầng App lấy access token mới.</summary>
public sealed record AppRefreshTokenCommand : ICommand
{
    public required string RefreshToken { get; init; }
}
