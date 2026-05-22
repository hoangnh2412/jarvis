using System.Security.Claims;
using Microsoft.Extensions.Options;

namespace Jarvis.Authentication.Basic;

public sealed class ConfigBasicCredentialValidator(IOptionsMonitor<AuthenticationBasicOption> options) : IBasicCredentialValidator
{
    public Task<BasicValidationResult?> ValidateAsync(
        string schemeName,
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        var scheme = options.Get(schemeName);
        if (!scheme.Users.TryGetValue(username, out var user))
            return Task.FromResult<BasicValidationResult?>(null);

        if (!string.Equals(user.Password, password, StringComparison.Ordinal))
            return Task.FromResult<BasicValidationResult?>(null);

        var claims = new List<Claim> { new(ClaimTypes.Name, username) };
        foreach (var role in user.Roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        return Task.FromResult<BasicValidationResult?>(new BasicValidationResult
        {
            Username = username,
            Claims = claims
        });
    }
}
