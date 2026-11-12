namespace Platform.Modules.Account.Commands;

public sealed record LoginResponse
{
    public required string AccessToken { get; init; }

    public required string RefreshToken { get; init; }

    public required Guid UserId { get; init; }
}
