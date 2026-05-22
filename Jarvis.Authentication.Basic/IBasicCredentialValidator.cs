using System.Security.Claims;

namespace Jarvis.Authentication.Basic;

public sealed class BasicValidationResult
{
    public required string Username { get; init; }

    public IReadOnlyList<Claim> Claims { get; init; } = [];
}

public interface IBasicCredentialValidator
{
    Task<BasicValidationResult?> ValidateAsync(
        string schemeName,
        string username,
        string password,
        CancellationToken cancellationToken = default);
}
