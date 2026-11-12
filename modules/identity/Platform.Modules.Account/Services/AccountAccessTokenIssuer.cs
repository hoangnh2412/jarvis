using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Platform.Authentication;
using Platform.Authentication.Identity.Domain;
using Platform.Authentication.Jwt;

namespace Platform.Modules.Account.Services;

/// <summary>Phát access token. Handler tự chọn realm và có gắn claim tenant hay không.</summary>
public sealed class AccountAccessTokenIssuer(
    IOptionsMonitor<AuthenticationJwtOption> jwtOptions,
    IConfiguration configuration)
{
    public string Issue(User user, string realm, bool includeTenantClaim)
    {
        var jwtOption = jwtOptions.Get(PlatformAuthenticationSchemes.Bearer);
        if (jwtOption is null || jwtOption.IssuerSigningKeys.Length == 0)
            throw new InvalidOperationException("JWT signing key not configured");

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOption.IssuerSigningKeys[0]));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("sub", user.Id.ToString()),
            new("jti", Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new("realm", realm),
        };

        if (includeTenantClaim && user.TenantId is Guid tenantId)
        {
            var tenantClaimName = configuration.GetValue<string>("TenantClaimName") ?? ClaimTypes.GroupSid;
            claims.Add(new Claim(tenantClaimName, tenantId.ToString()));
        }

        var lifetime = jwtOption.MaxExpireMinutes > 0
            ? TimeSpan.FromMinutes(jwtOption.MaxExpireMinutes)
            : TimeSpan.FromHours(1);
        var issuer = jwtOption.ValidIssuers.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
        var audience = !string.IsNullOrWhiteSpace(jwtOption.Audience)
            ? jwtOption.Audience
            : jwtOption.ValidAudiences.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.Add(lifetime),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
