using System.Security.Claims;

namespace Platform.Authentication.Basic;

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

    /// <summary>Build principal sau khi provider đã verify password (hash / storage).</summary>
    public static BasicValidationResult FromVerifiedUser(string username, IEnumerable<string> roles) =>
        FromVerifiedUser(username, roles, subjectId: null);

    /// <summary><paramref name="subjectId"/> — map <c>sub</c> / <c>NameIdentifier</c> (vd. user id cho JWT).</summary>
    public static BasicValidationResult FromVerifiedUser(string username, IEnumerable<string> roles, string? subjectId)
    {
        var claims = new List<Claim> { new(ClaimTypes.Name, username) };
        if (!string.IsNullOrWhiteSpace(subjectId))
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, subjectId));
            claims.Add(new Claim("sub", subjectId));
        }

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        return new BasicValidationResult
        {
            Username = username,
            Claims = claims
        };
    }
}
