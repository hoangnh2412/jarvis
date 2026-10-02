namespace Platform.Modules.Account.Queries;

using Platform.DDD.Domain.Shared.Messaging;

public sealed record GetMyProfileQuery : IQuery;

public sealed record MyProfileResponse
{
    public required Guid UserId { get; init; }
    public required string Username { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
}
