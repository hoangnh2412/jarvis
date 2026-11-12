using Platform.DDD.Domain.Shared.Messaging;

namespace Platform.Modules.Account.Commands;

/// <summary>Đăng nhập tầng Platform: user root, không tenant và không org.</summary>
public sealed record PlatformLoginCommand : ICommand
{
    public required string Username { get; init; }

    public required string Password { get; init; }
}
