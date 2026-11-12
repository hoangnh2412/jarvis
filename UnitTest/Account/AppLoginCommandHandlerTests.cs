namespace UnitTest.Account;

using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Platform.Authentication.Identity.Domain;
using Platform.Authentication.Jwt;
using Platform.Authorization.EntityFramework.Persistence;
using Platform.DDD.Domain.Shared.ExceptionHandling;
using Platform.Modules.Account;
using Platform.Modules.Account.Commands;
using Platform.Modules.Account.Services;
using Platform.Orgs.Abstractions;
using Platform.Tenants.Models;
using Platform.Tenants.Services;

public sealed class AppLoginCommandHandlerTests
{
    private const string SigningKey = "Sample-Test-DevSigningKey-32bit!";

    [Fact]
    public async Task HandleAsync_throws_unauthorized_when_tenant_code_is_unknown()
    {
        var tenants = new Mock<ITenantManager>();
        tenants.Setup(manager => manager.GetByCodeAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((TenantDescriptor?)null);

        var handler = new AppLoginCommandHandler(
            tenants.Object,
            new Mock<IOrgCatalog>().Object,
            new Mock<IAppIdentityStore>().Object,
            new PasswordHasher<User>(),
            CreateIssuer());

        var ex = await Assert.ThrowsAsync<BusinessException>(() => handler.HandleAsync(
            new AppLoginCommand
            {
                TenantCode = "missing",
                OrgCode = "default",
                UserName = "sampleuser",
                Password = "samplepass",
            },
            CancellationToken.None));
        Assert.Equal(AccountErrorCode.TenantUnavailable, ex.Code);
    }

    [Fact(DisplayName = "TenantId column is emitted and read back as ClaimTypes.GroupSid (default TenantClaimName)")]
    public async Task HandleAsync_emits_app_realm_and_tenant_claim()
    {
        var tenantId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var identity = await LoginAsync(tenantId, orgId, CreateConfiguration());

        Assert.Equal("app", identity.FindFirst("realm")?.Value);
        Assert.Equal(tenantId.ToString(), identity.FindFirst(ClaimTypes.GroupSid)?.Value);
    }

    [Fact(DisplayName = "Custom TenantClaimName is honoured")]
    public async Task HandleAsync_uses_configured_tenant_claim_name()
    {
        var tenantId = Guid.NewGuid();
        var identity = await LoginAsync(tenantId, Guid.NewGuid(), CreateConfiguration("tenant_id"));

        Assert.Equal(tenantId.ToString(), identity.FindFirst("tenant_id")?.Value);
        Assert.Null(identity.FindFirst(ClaimTypes.GroupSid));
    }

    private static async Task<ClaimsIdentity> LoginAsync(Guid tenantId, Guid orgId, IConfiguration configuration)
    {
        await using var db = new AppLoginTestDbContext(
            new DbContextOptionsBuilder<AppLoginTestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = "sampleuser",
            NormalizedUserName = "SAMPLEUSER",
            TenantId = tenantId,
            OrgId = orgId,
        };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, "correct-password");
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var tenants = new Mock<ITenantManager>();
        tenants.Setup(manager => manager.GetByCodeAsync("T1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TenantDescriptor { Id = tenantId, Code = "T1", Name = "T1", IsActive = true });
        var orgs = new Mock<IOrgCatalog>();
        orgs.Setup(catalog => catalog.GetByCodeAsync("default", tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrgDescriptor { Id = orgId, TenantId = tenantId, Code = "default", Name = "Default", IsActive = true });
        var store = new Mock<IAppIdentityStore>();
        store.Setup(item => item.DbContext).Returns(db);

        var handler = new AppLoginCommandHandler(
            tenants.Object,
            orgs.Object,
            store.Object,
            new PasswordHasher<User>(),
            CreateIssuer(configuration));
        var result = await handler.HandleAsync(
            new AppLoginCommand
            {
                TenantCode = "T1",
                OrgCode = "default",
                UserName = "sampleuser",
                Password = "correct-password",
            },
            CancellationToken.None);

        var validation = await new JsonWebTokenHandler { MapInboundClaims = true }.ValidateTokenAsync(
            result.AccessToken,
            new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)),
            });
        Assert.True(validation.IsValid, validation.Exception?.Message);
        return validation.ClaimsIdentity;
    }

    private static AccountAccessTokenIssuer CreateIssuer(IConfiguration? configuration = null)
    {
        var option = new AuthenticationJwtOption { IssuerSigningKeys = [SigningKey] };
        var monitor = new Mock<IOptionsMonitor<AuthenticationJwtOption>>();
        monitor.Setup(item => item.Get("Bearer")).Returns(option);
        return new AccountAccessTokenIssuer(monitor.Object, configuration ?? CreateConfiguration());
    }

    private static IConfiguration CreateConfiguration(string? tenantClaimName = null) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(tenantClaimName is null
                ? []
                : new Dictionary<string, string?> { ["TenantClaimName"] = tenantClaimName })
            .Build();

    private sealed class AppLoginTestDbContext(DbContextOptions<AppLoginTestDbContext> options)
        : IdentityDbContextBase(options);
}
