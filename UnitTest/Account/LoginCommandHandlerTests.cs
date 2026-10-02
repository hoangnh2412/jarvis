namespace UnitTest.Account;

using System.Security.Claims;
using System.Text;
using Platform.Modules.Account.Commands;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Platform.Authentication.Identity.Domain;
using Platform.Authentication.Jwt;

public sealed class LoginCommandHandlerTests
{
    private static IOptionsMonitor<AuthenticationJwtOption> CreateJwtOptions(string[]? signingKeys = null)
    {
        var option = new AuthenticationJwtOption
        {
            IssuerSigningKeys = signingKeys ?? ["Lexora-Test-DevSigningKey-32bit!"]
        };

        var monitor = new Mock<IOptionsMonitor<AuthenticationJwtOption>>();
        monitor.Setup(m => m.Get("Bearer")).Returns(option);
        return monitor.Object;
    }

    private static IConfiguration CreateConfiguration(string? tenantClaimName = null) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(tenantClaimName is null
                ? []
                : new Dictionary<string, string?> { ["TenantClaimName"] = tenantClaimName })
            .Build();

    /// <summary>
    /// Validates the token the way JwtBearer does by default (JsonWebTokenHandler, MapInboundClaims = true).
    /// </summary>
    private static async Task<ClaimsIdentity> ValidateAsJwtBearerAsync(string token)
    {
        var result = await new JsonWebTokenHandler { MapInboundClaims = true }.ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Lexora-Test-DevSigningKey-32bit!"))
        });

        Assert.True(result.IsValid, result.Exception?.Message);
        return result.ClaimsIdentity;
    }

    private static async Task<string> LoginAsync(User user, IConfiguration configuration)
    {
        var userManager = IdentityMockHelpers.MockUserManager<User>();
        userManager.Setup(m => m.FindByNameAsync(user.UserName!)).ReturnsAsync(user);
        userManager.Setup(m => m.CheckPasswordAsync(user, "correct-password")).ReturnsAsync(true);

        var handler = new LoginCommandHandler(userManager.Object, CreateJwtOptions(), configuration);
        var result = await handler.HandleAsync(
            new LoginCommand { Username = user.UserName!, Password = "correct-password" },
            CancellationToken.None);
        return result.AccessToken;
    }

    [Fact(DisplayName = "TenantId column is emitted and read back as ClaimTypes.GroupSid (default TenantClaimName)")]
    public async Task HandleAsync_emits_tenant_claim_readable_by_jwt_bearer()
    {
        var tenantId = Guid.NewGuid();
        var token = await LoginAsync(new User { Id = Guid.NewGuid(), UserName = "tenant.user", TenantId = tenantId }, CreateConfiguration());

        var identity = await ValidateAsJwtBearerAsync(token);

        Assert.Equal(tenantId.ToString(), identity.FindFirst(ClaimTypes.GroupSid)?.Value);
    }

    [Fact(DisplayName = "Custom TenantClaimName is honoured")]
    public async Task HandleAsync_uses_configured_tenant_claim_name()
    {
        var tenantId = Guid.NewGuid();
        var token = await LoginAsync(new User { Id = Guid.NewGuid(), UserName = "tenant.user", TenantId = tenantId }, CreateConfiguration("tenant_id"));

        var identity = await ValidateAsJwtBearerAsync(token);

        Assert.Equal(tenantId.ToString(), identity.FindFirst("tenant_id")?.Value);
        Assert.Null(identity.FindFirst(ClaimTypes.GroupSid));
    }

    [Fact(DisplayName = "User without TenantId (global Admin) gets no tenant claim")]
    public async Task HandleAsync_omits_tenant_claim_when_user_has_no_tenant()
    {
        var token = await LoginAsync(new User { Id = Guid.NewGuid(), UserName = "sys.admin", TenantId = null }, CreateConfiguration());

        var identity = await ValidateAsJwtBearerAsync(token);

        Assert.Null(identity.FindFirst(ClaimTypes.GroupSid));
    }

    [Fact]
    public async Task HandleAsync_returns_access_token_for_valid_credentials()
    {
        var user = new User { Id = Guid.NewGuid(), UserName = "hoang" };
        var userManager = IdentityMockHelpers.MockUserManager<User>();
        userManager.Setup(m => m.FindByNameAsync("hoang")).ReturnsAsync(user);
        userManager.Setup(m => m.CheckPasswordAsync(user, "correct-password")).ReturnsAsync(true);

        var handler = new LoginCommandHandler(userManager.Object, CreateJwtOptions(), CreateConfiguration());

        var result = await handler.HandleAsync(
            new LoginCommand { Username = "hoang", Password = "correct-password" },
            CancellationToken.None);

        Assert.Equal(user.Id, result.UserId);
        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
    }

    [Fact]
    public async Task HandleAsync_throws_unauthorized_when_username_not_found()
    {
        var userManager = IdentityMockHelpers.MockUserManager<User>();
        userManager.Setup(m => m.FindByNameAsync("ghost")).ReturnsAsync((User?)null);

        var handler = new LoginCommandHandler(userManager.Object, CreateJwtOptions(), CreateConfiguration());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.HandleAsync(
            new LoginCommand { Username = "ghost", Password = "anything" },
            CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_throws_unauthorized_when_password_invalid()
    {
        var user = new User { Id = Guid.NewGuid(), UserName = "hoang" };
        var userManager = IdentityMockHelpers.MockUserManager<User>();
        userManager.Setup(m => m.FindByNameAsync("hoang")).ReturnsAsync(user);
        userManager.Setup(m => m.CheckPasswordAsync(user, "wrong-password")).ReturnsAsync(false);

        var handler = new LoginCommandHandler(userManager.Object, CreateJwtOptions(), CreateConfiguration());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.HandleAsync(
            new LoginCommand { Username = "hoang", Password = "wrong-password" },
            CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_throws_unauthorized_when_account_locked_even_with_correct_password()
    {
        var user = new User { Id = Guid.NewGuid(), UserName = "locked" };
        var userManager = IdentityMockHelpers.MockUserManager<User>();
        userManager.Setup(m => m.FindByNameAsync("locked")).ReturnsAsync(user);
        userManager.Setup(m => m.IsLockedOutAsync(user)).ReturnsAsync(true);
        userManager.Setup(m => m.CheckPasswordAsync(user, "correct-password")).ReturnsAsync(true);

        var handler = new LoginCommandHandler(userManager.Object, CreateJwtOptions(), CreateConfiguration());

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.HandleAsync(
            new LoginCommand { Username = "locked", Password = "correct-password" },
            CancellationToken.None));

        Assert.Equal("Account is locked", ex.Message);
        userManager.Verify(m => m.CheckPasswordAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_returns_token_when_lockout_has_expired()
    {
        var user = new User { Id = Guid.NewGuid(), UserName = "unlocked" };
        var userManager = IdentityMockHelpers.MockUserManager<User>();
        userManager.Setup(m => m.FindByNameAsync("unlocked")).ReturnsAsync(user);
        userManager.Setup(m => m.IsLockedOutAsync(user)).ReturnsAsync(false);
        userManager.Setup(m => m.CheckPasswordAsync(user, "correct-password")).ReturnsAsync(true);

        var handler = new LoginCommandHandler(userManager.Object, CreateJwtOptions(), CreateConfiguration());

        var result = await handler.HandleAsync(
            new LoginCommand { Username = "unlocked", Password = "correct-password" },
            CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
    }

    [Fact]
    public async Task HandleAsync_throws_when_jwt_signing_key_not_configured()
    {
        var user = new User { Id = Guid.NewGuid(), UserName = "hoang" };
        var userManager = IdentityMockHelpers.MockUserManager<User>();
        userManager.Setup(m => m.FindByNameAsync("hoang")).ReturnsAsync(user);
        userManager.Setup(m => m.CheckPasswordAsync(user, "correct-password")).ReturnsAsync(true);

        var handler = new LoginCommandHandler(userManager.Object, CreateJwtOptions(signingKeys: []), CreateConfiguration());

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(
            new LoginCommand { Username = "hoang", Password = "correct-password" },
            CancellationToken.None));
    }
}
