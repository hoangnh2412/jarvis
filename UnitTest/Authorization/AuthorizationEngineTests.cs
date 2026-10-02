using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Platform.Authorization;
using Platform.Authorization.Abstractions;
using Platform.Authorization.Domain;
using Platform.Authorization.EntityFramework;
using Platform.Authorization.Services;
using Xunit;

namespace UnitTest.Authorization;

public sealed class AuthorizationEngineTests
{
    private readonly Mock<IPermissionProvider> _permissionProviderMock;
    private readonly Mock<IPolicyProvider> _policyProviderMock;
    private readonly Mock<IPolicyEvaluator> _policyEvaluatorMock;
    private readonly IAuthorizationEngine _engine;

    public AuthorizationEngineTests()
    {
        _permissionProviderMock = new Mock<IPermissionProvider>();
        _policyProviderMock = new Mock<IPolicyProvider>();
        _policyEvaluatorMock = new Mock<IPolicyEvaluator>();

        // Default mock behaviors
        _permissionProviderMock
            .Setup(p => p.GetPermissionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string>());

        _permissionProviderMock
            .Setup(p => p.HasPermissionAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _permissionProviderMock
            .Setup(p => p.GetPermissionsByRoleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ISet<string>>());

        _policyProviderMock
            .Setup(p => p.GetPoliciesByUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Policy>());

        _policyProviderMock
            .Setup(p => p.GetPoliciesByRoleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Policy>());

        _engine = new AuthorizationEngine(
            _permissionProviderMock.Object,
            _policyProviderMock.Object,
            _policyEvaluatorMock.Object);
    }

    #region T1 - Smoke Test

    [Fact(DisplayName = "T1: Host registration without error")]
    public void T1_HostRegistrationSucceeds()
    {
        // Arrange
        var builder = Host.CreateApplicationBuilder();

        // Register in-memory database for testing
        builder.Services.AddDbContext<UnitTest.Authorization.Helpers.InMemoryTestDbContext>(
            options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()),
            ServiceLifetime.Scoped);

        // Register abstract service with concrete implementation for DI
        builder.Services.AddScoped<Platform.Authorization.EntityFramework.Persistence.IdentityDbContextBase>(
            sp => sp.GetRequiredService<UnitTest.Authorization.Helpers.InMemoryTestDbContext>());

        builder.AddPlatformAuthorization();
        builder.AddPlatformAuthorizationEntityFramework();
        var host = builder.Build();

        // Act & Assert
        Assert.NotNull(host);
        Assert.NotNull(host.Services.GetService(typeof(IAuthorizationEngine)));
        Assert.NotNull(host.Services.GetService(typeof(IPermissionProvider)));
        Assert.NotNull(host.Services.GetService(typeof(IPolicyProvider)));
        Assert.NotNull(host.Services.GetService(typeof(IPolicyEvaluator)));
        Assert.NotNull(host.Services.GetService(typeof(IOperandResolver)));
        Assert.NotNull(host.Services.GetService(typeof(IRoleGovernanceValidator)));
    }

    [Fact(DisplayName = "T1: engine core registers without any store package")]
    public void T1_CoreRegistrationIsStoreAgnostic()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.AddPlatformAuthorization();
        var services = builder.Services;

        Assert.Contains(services, d => d.ServiceType == typeof(IAuthorizationEngine));
        Assert.DoesNotContain(services, d => d.ServiceType == typeof(IPermissionProvider));
        Assert.DoesNotContain(services, d => d.ServiceType == typeof(IRoleGovernanceValidator));
    }

    #endregion

    #region T2-T3 - Permission Checks

    [Fact(DisplayName = "T2: Wildcard permission check returns true")]
    public async Task T2_WildcardPermissionReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _permissionProviderMock
            .Setup(p => p.HasPermissionAsync(userId, "*", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _engine.HasPermissionAsync(userId, "*");

        // Assert
        Assert.True(result);
    }

    [Fact(DisplayName = "T3: Specific permission check")]
    public async Task T3_SpecificPermissionCheck()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _permissionProviderMock
            .Setup(p => p.HasPermissionAsync(userId, "Expense.View", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _engine.HasPermissionAsync(userId, "Expense.View");

        // Assert
        Assert.True(result);
    }

    #endregion

    #region T4-T6 - Authorization Checks

    [Fact(DisplayName = "T4: RBAC deny → short-circuit, no ABAC evaluation")]
    public async Task T4_RbacDenyShortCircuits()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var testResource = new { OrgId = Guid.NewGuid() };

        _permissionProviderMock
            .Setup(p => p.HasPermissionAsync(userId, "Expense.Approve", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _engine.AuthorizeAsync(userId, "Expense.Approve", testResource);

        // Assert
        Assert.False(result);
        _policyProviderMock.Verify(
            p => p.GetPoliciesByUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "Policy provider should not be called when RBAC check fails");
        _policyProviderMock.Verify(
            p => p.GetPoliciesByRoleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "Policy provider should not be called when RBAC check fails");
    }

    [Fact(DisplayName = "T5: Admin bypass with System.All permission")]
    public async Task T5_AdminBypassAbac()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var testResource = new { OrgId = Guid.NewGuid() };

        _permissionProviderMock
            .Setup(p => p.GetPermissionsByRoleAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ISet<string>>
            {
                [Guid.NewGuid()] = new HashSet<string> { "System.All" }
            });

        // Act
        var result = await _engine.EvaluatePoliciesAsync(userId, "Expense.Approve", testResource);

        // Assert
        Assert.True(result.IsAllowed);
        Assert.Equal("System.All", result.PolicyCode);
    }

    [Fact(DisplayName = "T6: ABAC evaluation with no policies assigned")]
    public async Task T6_NoPoliciesAssignedDeny()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var testResource = new { OrgId = Guid.NewGuid() };

        var roleId = Guid.NewGuid();
        _permissionProviderMock
            .Setup(p => p.GetPermissionsByRoleAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ISet<string>>
            {
                [roleId] = new HashSet<string> { "Expense.View" }
            });

        _policyProviderMock
            .Setup(p => p.GetPoliciesByRoleAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Policy>());

        // Act
        var result = await _engine.EvaluatePoliciesAsync(userId, "Expense.View", testResource);

        // Assert
        Assert.False(result.IsAllowed);
        Assert.False(string.IsNullOrWhiteSpace(result.Reason));
    }

    #endregion

    #region T10 - AuthorizeAsync Integration

    [Fact(DisplayName = "T10: Authorize without resource (RBAC only)")]
    public async Task T10_AuthorizeRbacOnly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _permissionProviderMock
            .Setup(p => p.HasPermissionAsync(userId, "Employee.View", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _engine.AuthorizeAsync(userId, "Employee.View", resource: null);

        // Assert
        Assert.True(result);
        _policyProviderMock.Verify(
            p => p.GetPoliciesByUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "Policy evaluation should not happen when resource is null");
    }

    #endregion
}
