using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Platform.Authentication;
using Platform.Authentication.Basic;
using Platform.Authentication.Jwt;

namespace Sample.Authentication;

/// <summary>JWT access token — ký bằng <see cref="AuthenticationJwtOption.IssuerSigningKeys"/> của scheme Bearer.</summary>
public sealed class DevBearerTokenIssuer(IOptionsMonitor<AuthenticationJwtOption> jwtOptions)
{
    public DevBearerTokenResult IssueFromValidation(BasicValidationResult validation, int expiresInMinutes = 0)
    {
        var jwtOption = jwtOptions.Get(PlatformAuthenticationSchemes.Bearer);
        if (jwtOption.IssuerSigningKeys.Length == 0)
            throw new InvalidOperationException(
                "Authentication:Schemes:Bearer:Profiles:Default:IssuerSigningKeys is required to sign access tokens.");

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOption.IssuerSigningKeys[0]));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>(validation.Claims)
        {
            new("jti", Guid.NewGuid().ToString()),
        };

        if (claims.All(c => c.Type != ClaimTypes.Name))
            claims.Add(new Claim(ClaimTypes.Name, validation.Username));

        var lifetime = expiresInMinutes > 0
            ? TimeSpan.FromMinutes(expiresInMinutes)
            : jwtOption.MaxExpireMinutes > 0
                ? TimeSpan.FromMinutes(jwtOption.MaxExpireMinutes)
                : TimeSpan.FromHours(1);

        var issuer = jwtOption.ValidIssuers.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
        var audience = !string.IsNullOrWhiteSpace(jwtOption.Audience)
            ? jwtOption.Audience
            : jwtOption.ValidAudiences.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

        var expires = DateTime.UtcNow.Add(lifetime);
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return new DevBearerTokenResult(
            new JwtSecurityTokenHandler().WriteToken(token),
            PlatformAuthenticationSchemes.Bearer,
            expires,
            audience);
    }
}

public sealed record DevBearerTokenResult(
    string AccessToken,
    string TokenType,
    DateTime ExpiresAtUtc,
    string? Audience);
