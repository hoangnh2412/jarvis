namespace Platform.Modules.Account.Commands;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Platform.Authentication.Identity.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
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
            throw new UnauthorizedAccessException("Invalid username or password");

        var token = await GenerateAccessTokenAsync(user, cancellationToken);

        return new LoginResponse
        {
            AccessToken = token,
            RefreshToken = string.Empty,
            UserId = user.Id
        };
    }

    private async Task<string> GenerateAccessTokenAsync(User user, CancellationToken cancellationToken)
    {
        var jwtOption = _jwtOptions.Get("Bearer");
        if (jwtOption is null || jwtOption.IssuerSigningKeys.Length == 0)
            throw new InvalidOperationException("JWT signing key not configured");

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOption.IssuerSigningKeys[0]));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("sub", user.Id.ToString()),
            new("jti", Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, user.UserName ?? string.Empty)
        };

        if (user.TenantId is { } tenantId)
            claims.Add(new Claim(_tenantClaimName, tenantId.ToString()));

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
