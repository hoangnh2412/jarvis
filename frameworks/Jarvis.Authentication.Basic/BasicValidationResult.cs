using System.Security.Claims;

namespace Jarvis.Authentication.Basic;

/// <summary>Kết quả validate credential Basic — chứa username và claims gán cho principal.</summary>
public sealed class BasicValidationResult
{
    public required string Username { get; init; }

    public IReadOnlyList<Claim> Claims { get; init; } = [];

    /// <summary>So khớp password plain-text và build claims — dùng từ provider đọc config.</summary>
    public static BasicValidationResult? Validate(string username, string password, IBasicUserCredential credential)
    {
        if (string.IsNullOrEmpty(credential.Password))
            return null;

        if (!string.Equals(credential.Password, password, StringComparison.Ordinal))
            return null;

        var claims = new List<Claim> { new(ClaimTypes.Name, username) };
        foreach (var role in credential.Roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        return new BasicValidationResult
        {
            Username = username,
            Claims = claims
        };
    }
}
