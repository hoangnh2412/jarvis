using System.Security.Claims;

namespace Jarvis.Authentication.Basic;

internal static class BasicCredentialValidation
{
    internal static BasicValidationResult? Validate(string username, string password, BasicUserCredential credential)
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
