namespace Platform.Modules.Account.Commands;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Platform.Authentication.Identity.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Platform.Authentication;
using Platform.Authentication.Jwt;
using Platform.DDD.Application.Contracts.Commands;

public sealed class LoginCommandHandler : IAsyncCommandHandler<LoginCommand, LoginResponse>
{
    private readonly UserManager<User> _userManager;
    private readonly IOptionsMonitor<AuthenticationJwtOption> _jwtOptions;
    private readonly string _tenantClaimName;

    public LoginCommandHandler(
        UserManager<User> userManager,
        IOptionsMonitor<AuthenticationJwtOption> jwtOptions,
        IConfiguration configuration)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _jwtOptions = jwtOptions ?? throw new ArgumentNullException(nameof(jwtOptions));
        ArgumentNullException.ThrowIfNull(configuration);

        // Must match the readers (CurrentUserStore, UserTenantIdResolver, NotificationHub).
        _tenantClaimName = configuration.GetValue<string>("TenantClaimName") ?? ClaimTypes.GroupSid;
    }

    public async Task<LoginResponse> HandleAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByNameAsync(command.Username);
        if (user == null)
            throw new UnauthorizedAccessException("Invalid username or password");

        // CheckPasswordAsync does not enforce lockout; mirror SignInManager by checking it first.
        if (await _userManager.IsLockedOutAsync(user))
            throw new UnauthorizedAccessException("Account is locked");

        var passwordValid = await _userManager.CheckPasswordAsync(user, command.Password);
        if (!passwordValid)
        {
            await _userManager.AccessFailedAsync(user);
            if (await _userManager.IsLockedOutAsync(user))
                throw new UnauthorizedAccessException("Account is locked");

            throw new UnauthorizedAccessException("Invalid username or password");
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        var isPlatformAccount = user.TenantId is null && user.OrgId is null;
        var wantsPlatform = string.Equals(command.Realm, "platform", StringComparison.OrdinalIgnoreCase);
        if (wantsPlatform != isPlatformAccount)
            throw new UnauthorizedAccessException("Invalid username or password");

        var token = GenerateAccessToken(user, wantsPlatform ? "platform" : "app");

        return new LoginResponse
        {
            AccessToken = token,
            RefreshToken = string.Empty,
            UserId = user.Id
        };
    }

    private string GenerateAccessToken(User user, string realm)
    {
        var jwtOption = _jwtOptions.Get(PlatformAuthenticationSchemes.Bearer);
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

        if (user.TenantId is { } tenantId)
            claims.Add(new Claim(_tenantClaimName, tenantId.ToString()));

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
