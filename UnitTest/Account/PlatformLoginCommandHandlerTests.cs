namespace UnitTest.Account;

using System.Security.Claims;
using System.Text;
using Platform.DDD.Domain.Shared.ExceptionHandling;
using Platform.Modules.Account;
using Platform.Modules.Account.Commands;
using Platform.Modules.Account.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Platform.Authentication.Identity.Domain;
using Platform.Authentication.Jwt;

public sealed class PlatformLoginCommandHandlerTests
{
    private static IOptionsMonitor<AuthenticationJwtOption> CreateJwtOptions(string[]? signingKeys = null)
    {
        var option = new AuthenticationJwtOption
        {
            IssuerSigningKeys = signingKeys ?? ["Sample-Test-DevSigningKey-32bit!"]
        };

        var monitor = new Mock<IOptionsMonitor<AuthenticationJwtOption>>();
        monitor.Setup(m => m.Get("Bearer")).Returns(option);
        return monitor.Object;
    }

    private static IConfiguration CreateConfiguration() =>
        new ConfigurationBuilder().AddInMemoryCollection().Build();

    private static AccountAccessTokenIssuer CreateIssuer(
        IOptionsMonitor<AuthenticationJwtOption>? jwtOptions = null,
        IConfiguration? configuration = null) =>
        new(jwtOptions ?? CreateJwtOptions(), configuration ?? CreateConfiguration());

    private static async Task<ClaimsIdentity> ValidateAsJwtBearerAsync(string token)
    {
        var result = await new JsonWebTokenHandler { MapInboundClaims = true }.ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Sample-Test-DevSigningKey-32bit!"))
        });

        Assert.True(result.IsValid, result.Exception?.Message);
        return result.ClaimsIdentity;
    }

    private static PlatformLoginCommandHandler CreateHandler(
        Mock<UserManager<User>> userManager,
        IOptionsMonitor<AuthenticationJwtOption>? jwtOptions = null,
        IConfiguration? configuration = null) =>
        new(userManager.Object, CreateIssuer(jwtOptions, configuration));

    [Fact(DisplayName = "User without TenantId (global Admin) gets no tenant claim")]
    public async Task HandleAsync_omits_tenant_claim_when_user_has_no_tenant()
    {
        var user = new User { Id = Guid.NewGuid(), UserName = "sys.admin", TenantId = null };
        var userManager = IdentityMockHelpers.MockUserManager<User>();
        userManager.Setup(m => m.FindByNameAsync(user.UserName!)).ReturnsAsync(user);
        userManager.Setup(m => m.CheckPasswordAsync(user, "correct-password")).ReturnsAsync(true);

        var handler = CreateHandler(userManager);
        var result = await handler.HandleAsync(
            new PlatformLoginCommand { Username = user.UserName!, Password = "correct-password" },
            CancellationToken.None);

        var identity = await ValidateAsJwtBearerAsync(result.AccessToken);
        Assert.Null(identity.FindFirst(ClaimTypes.GroupSid));
        Assert.Equal("platform", identity.FindFirst("realm")?.Value);
    }

    [Fact]
    public async Task HandleAsync_throws_unauthorized_when_user_has_tenant()
    {
        var user = new User { Id = Guid.NewGuid(), UserName = "tenant.user", TenantId = Guid.NewGuid() };
        var userManager = IdentityMockHelpers.MockUserManager<User>();
        userManager.Setup(m => m.FindByNameAsync("tenant.user")).ReturnsAsync(user);
        userManager.Setup(m => m.CheckPasswordAsync(user, "correct-password")).ReturnsAsync(true);

        var handler = CreateHandler(userManager);

        var ex = await Assert.ThrowsAsync<BusinessException>(() => handler.HandleAsync(
            new PlatformLoginCommand { Username = "tenant.user", Password = "correct-password" },
            CancellationToken.None));
        Assert.Equal(AccountErrorCode.NotPlatformAccount, ex.Code);
    }

    [Fact]
    public async Task HandleAsync_returns_access_token_for_valid_credentials()
    {
        var user = new User { Id = Guid.NewGuid(), UserName = "hoang" };
        var userManager = IdentityMockHelpers.MockUserManager<User>();
        userManager.Setup(m => m.FindByNameAsync("hoang")).ReturnsAsync(user);
        userManager.Setup(m => m.CheckPasswordAsync(user, "correct-password")).ReturnsAsync(true);

        var handler = CreateHandler(userManager);

        var result = await handler.HandleAsync(
            new PlatformLoginCommand { Username = "hoang", Password = "correct-password" },
            CancellationToken.None);

        Assert.Equal(user.Id, result.UserId);
        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
    }

    [Fact]
    public async Task HandleAsync_throws_unauthorized_when_username_not_found()
    {
        var userManager = IdentityMockHelpers.MockUserManager<User>();
        userManager.Setup(m => m.FindByNameAsync("ghost")).ReturnsAsync((User?)null);

        var handler = CreateHandler(userManager);

        var ex = await Assert.ThrowsAsync<BusinessException>(() => handler.HandleAsync(
            new PlatformLoginCommand { Username = "ghost", Password = "anything" },
            CancellationToken.None));
        Assert.Equal(AccountErrorCode.InvalidCredentials, ex.Code);
    }

    [Fact]
    public async Task HandleAsync_throws_unauthorized_when_password_invalid()
    {
        var user = new User { Id = Guid.NewGuid(), UserName = "hoang" };
        var userManager = IdentityMockHelpers.MockUserManager<User>();
        userManager.Setup(m => m.FindByNameAsync("hoang")).ReturnsAsync(user);
        userManager.Setup(m => m.CheckPasswordAsync(user, "wrong-password")).ReturnsAsync(false);

        var handler = CreateHandler(userManager);

        var ex = await Assert.ThrowsAsync<BusinessException>(() => handler.HandleAsync(
            new PlatformLoginCommand { Username = "hoang", Password = "wrong-password" },
            CancellationToken.None));
        Assert.Equal(AccountErrorCode.InvalidCredentials, ex.Code);
    }

    [Fact]
    public async Task HandleAsync_throws_unauthorized_when_account_locked_even_with_correct_password()
    {
        var user = new User { Id = Guid.NewGuid(), UserName = "locked" };
        var userManager = IdentityMockHelpers.MockUserManager<User>();
        userManager.Setup(m => m.FindByNameAsync("locked")).ReturnsAsync(user);
        userManager.Setup(m => m.IsLockedOutAsync(user)).ReturnsAsync(true);
        userManager.Setup(m => m.CheckPasswordAsync(user, "correct-password")).ReturnsAsync(true);

        var handler = CreateHandler(userManager);

        var ex = await Assert.ThrowsAsync<BusinessException>(() => handler.HandleAsync(
            new PlatformLoginCommand { Username = "locked", Password = "correct-password" },
            CancellationToken.None));

        Assert.Equal(AccountErrorCode.AccountLocked, ex.Code);
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

        var handler = CreateHandler(userManager);

        var result = await handler.HandleAsync(
            new PlatformLoginCommand { Username = "unlocked", Password = "correct-password" },
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

        var handler = CreateHandler(userManager, CreateJwtOptions(signingKeys: []));

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(
            new PlatformLoginCommand { Username = "hoang", Password = "correct-password" },
            CancellationToken.None));
    }

    [Fact(DisplayName = "T90/T91: wrong password records a failure; success resets the counter")]
    public async Task HandleAsync_records_access_failure_and_resets_on_success()
    {
        var user = new User { Id = Guid.NewGuid(), UserName = "hoang" };
        var userManager = IdentityMockHelpers.MockUserManager<User>();
        userManager.Setup(m => m.FindByNameAsync("hoang")).ReturnsAsync(user);
        userManager.Setup(m => m.CheckPasswordAsync(user, "wrong-password")).ReturnsAsync(false);
        userManager.Setup(m => m.CheckPasswordAsync(user, "correct-password")).ReturnsAsync(true);

        var handler = CreateHandler(userManager);

        var ex = await Assert.ThrowsAsync<BusinessException>(() => handler.HandleAsync(
            new PlatformLoginCommand { Username = "hoang", Password = "wrong-password" },
            CancellationToken.None));
        Assert.Equal(AccountErrorCode.InvalidCredentials, ex.Code);
        userManager.Verify(m => m.AccessFailedAsync(user), Times.Once);

        await handler.HandleAsync(
            new PlatformLoginCommand { Username = "hoang", Password = "correct-password" },
            CancellationToken.None);
        userManager.Verify(m => m.ResetAccessFailedCountAsync(user), Times.Once);
    }

    [Fact(DisplayName = "T107: token carries configured issuer and audience")]
    public async Task HandleAsync_emits_configured_issuer_and_audience()
    {
        var option = new AuthenticationJwtOption
        {
            IssuerSigningKeys = ["Sample-Test-DevSigningKey-32bit!"],
            ValidIssuers = ["https://jarvis.local"],
            Audience = "jarvis-api",
            MaxExpireMinutes = 15,
        };
        var monitor = new Mock<IOptionsMonitor<AuthenticationJwtOption>>();
        monitor.Setup(m => m.Get("Bearer")).Returns(option);

        var user = new User { Id = Guid.NewGuid(), UserName = "hoang" };
        var userManager = IdentityMockHelpers.MockUserManager<User>();
        userManager.Setup(m => m.FindByNameAsync("hoang")).ReturnsAsync(user);
        userManager.Setup(m => m.CheckPasswordAsync(user, "correct-password")).ReturnsAsync(true);

        var handler = CreateHandler(userManager, monitor.Object);
        var result = await handler.HandleAsync(
            new PlatformLoginCommand { Username = "hoang", Password = "correct-password" },
            CancellationToken.None);

        var validation = await new JsonWebTokenHandler().ValidateTokenAsync(result.AccessToken, new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://jarvis.local",
            ValidateAudience = true,
            ValidAudience = "jarvis-api",
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Sample-Test-DevSigningKey-32bit!")),
        });

        Assert.True(validation.IsValid, validation.Exception?.Message);
    }
}
