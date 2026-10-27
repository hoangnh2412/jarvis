namespace Platform.Modules.Account.Commands;

using Platform.DDD.Domain.Shared.Messaging;

/// <summary>
/// Login command: authenticate user via username/password and issue an access token.
/// Permission and policy stay in the store; the token carries subject, jti, name and tenant only.
/// </summary>
public sealed record LoginCommand : ICommand
{
    public required string Username { get; init; }

    public required string Password { get; init; }

    /// <summary><c>platform</c> = user Master (không tenant/org). <c>app</c> = user đã gắn tenant.</summary>
    public string Realm { get; init; } = "app";
}

public sealed record LoginResponse
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required Guid UserId { get; init; }
}
