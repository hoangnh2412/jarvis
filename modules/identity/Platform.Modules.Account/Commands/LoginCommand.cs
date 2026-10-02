namespace Platform.Modules.Account.Commands;

using Platform.DDD.Domain.Shared.Messaging;

/// <summary>
/// Login command: authenticate user via username/password.
/// After successful auth, builds Authorization Context with permissions/policies.
/// </summary>
public sealed record LoginCommand : ICommand
{
    public required string Username { get; init; }
    public required string Password { get; init; }
}

public sealed record LoginResponse
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required Guid UserId { get; init; }
}
