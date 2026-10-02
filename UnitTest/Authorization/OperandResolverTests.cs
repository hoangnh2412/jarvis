using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Platform.Authentication.Identity.Domain;
using Platform.Authorization.EntityFramework.Persistence;
using Platform.Authorization.EntityFramework.Services;
using Platform.Authorization.Operands;
using UnitTest.Authorization.Helpers;
using Xunit;

namespace UnitTest.Authorization;

public sealed class OperandResolverTests
{
    private readonly OperandResolver _resolver;
    private readonly InMemoryTestDbContext _context;

    public OperandResolverTests()
    {
        var options = new DbContextOptionsBuilder<InMemoryTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new InMemoryTestDbContext(options);
        _resolver = new OperandResolver(_context, TestOperands.Registry());
    }

    #region T9 / D10 - Runtime whitelist enforcement

    [Theory(DisplayName = "D10: unregistered operand is rejected at runtime even if the property exists")]
    [InlineData("Resource.Password")]
    [InlineData("User.PasswordHash")]
    [InlineData("Resource.Secret")]
    public async Task D10_UnregisteredOperandRejected(string operand)
    {
        var resource = new { Password = "p@ss", Secret = "s" };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _resolver.ResolveAsync(operand, Guid.NewGuid(), resource));

        Assert.Contains(operand, ex.Message);
    }

    [Fact(DisplayName = "D10: business operand rejected when the product did not register it")]
    public async Task D10_BusinessOperandRequiresRegistration()
    {
        var platformOnly = new OperandResolver(_context, OperandRegistry.Build());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => platformOnly.ResolveAsync("Resource.Amount", Guid.NewGuid(), new TestResource { Amount = 1m }));
    }

    [Fact(DisplayName = "T9: Invalid operand format throws")]
    public async Task T9_InvalidFormatThrows()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var resource = new { OrgId = Guid.NewGuid() };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _resolver.ResolveAsync("InvalidFormat", userId, resource));
    }

    [Fact(DisplayName = "T9: Operand with multiple dots throws")]
    public async Task T9_MultipleDotsThrows()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var resource = new { OrgId = Guid.NewGuid() };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _resolver.ResolveAsync("User.Profile.Id", userId, resource));
    }

    #endregion

    #region T11-T13 - Operand Resolution

    [Fact(DisplayName = "T11: Resolve Resource operand via reflection")]
    public async Task T11_ResolveResourceOperand()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var testOrgId = Guid.NewGuid();
        var resource = new TestResource { OrgId = testOrgId };

        // Act
        var value = await _resolver.ResolveAsync("Resource.OrgId", userId, resource);

        // Assert
        Assert.Equal(testOrgId, value);
    }

    [Fact(DisplayName = "T12: Registered operand missing on this resource type returns null")]
    public async Task T12_ResolveMissingPropertyReturnsNull()
    {
        var resource = new TestResource { OrgId = Guid.NewGuid() };

        var value = await _resolver.ResolveAsync("Resource.ProjectId", Guid.NewGuid(), resource);

        Assert.Null(value);
    }

    [Fact(DisplayName = "T13: Resolve Resource without object throws")]
    public async Task T13_ResolveResourceWithoutObjectThrows()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _resolver.ResolveAsync("Resource.OrgId", userId, null));
    }

    #endregion

    #region S7 - User operand sources (ADR Q3)

    [Fact(DisplayName = "S7: User.Id resolves to the subject id without any claim")]
    public async Task S7_UserIdIsSubject()
    {
        var userId = Guid.NewGuid();

        Assert.Equal(userId, await _resolver.ResolveAsync("User.Id", userId));
    }

    [Fact(DisplayName = "S7: User.TenantId / User.OrgId come from AspNetUsers columns, not claims")]
    public async Task S7_TenantAndOrgFromColumns()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        _context.Users.Add(new User { Id = userId, UserName = "u", TenantId = tenantId, OrgId = orgId });
        _context.UserClaims.Add(new IdentityUserClaim<Guid> { UserId = userId, ClaimType = "TenantId", ClaimValue = Guid.NewGuid().ToString() });
        _context.UserClaims.Add(new IdentityUserClaim<Guid> { UserId = userId, ClaimType = "OrgId", ClaimValue = Guid.NewGuid().ToString() });
        await _context.SaveChangesAsync();

        Assert.Equal(tenantId, await _resolver.ResolveAsync("User.TenantId", userId));
        Assert.Equal(orgId, await _resolver.ResolveAsync("User.OrgId", userId));
    }

    [Fact(DisplayName = "S7: unknown user / empty column resolves to null")]
    public async Task S7_MissingUserOrColumnIsNull()
    {
        var userId = Guid.NewGuid();
        _context.Users.Add(new User { Id = userId, UserName = "global", TenantId = null, OrgId = null });
        await _context.SaveChangesAsync();

        Assert.Null(await _resolver.ResolveAsync("User.OrgId", userId));
        Assert.Null(await _resolver.ResolveAsync("User.TenantId", Guid.NewGuid()));
    }

    [Fact(DisplayName = "S7: DepartmentId / ApproveLimit still come from UserClaims")]
    public async Task S7_OtherAttributesFromClaims()
    {
        var userId = Guid.NewGuid();
        var deptId = Guid.NewGuid();
        _context.UserClaims.Add(new IdentityUserClaim<Guid> { UserId = userId, ClaimType = "DepartmentId", ClaimValue = deptId.ToString() });
        _context.UserClaims.Add(new IdentityUserClaim<Guid> { UserId = userId, ClaimType = "ApproveLimit", ClaimValue = "2000000" });
        await _context.SaveChangesAsync();

        Assert.Equal(deptId, await _resolver.ResolveAsync("User.DepartmentId", userId));
        Assert.Equal(2_000_000m, await _resolver.ResolveAsync("User.ApproveLimit", userId));
    }

    [Fact(DisplayName = "Collection claim (CSV) is parsed item by item so IN matches Guid resources")]
    public async Task CollectionClaim_ParsedAsGuids()
    {
        var userId = Guid.NewGuid();
        var d1 = Guid.NewGuid();
        var d2 = Guid.NewGuid();
        _context.UserClaims.Add(new IdentityUserClaim<Guid> { UserId = userId, ClaimType = "ManagedDepartmentIds", ClaimValue = $"{d1}, {d2}" });
        await _context.SaveChangesAsync();

        var value = await _resolver.ResolveAsync("User.ManagedDepartmentIds", userId);

        var items = Assert.IsType<List<object>>(value);
        Assert.Equal(new object[] { d1, d2 }, items);
    }

    [Fact(DisplayName = "Custom claim type: operand name can differ from the stored claim type")]
    public async Task CustomClaimType_IsUsed()
    {
        var userId = Guid.NewGuid();
        _context.UserClaims.Add(new IdentityUserClaim<Guid> { UserId = userId, ClaimType = "erp:approve_limit", ClaimValue = "5000000" });
        await _context.SaveChangesAsync();
        var resolver = new OperandResolver(_context, OperandRegistry.Build(o => o.AddUserClaim("ApproveLimit", claimType: "erp:approve_limit")));

        Assert.Equal(5_000_000m, await resolver.ResolveAsync("User.ApproveLimit", userId));
    }

    #endregion

    /// <summary>
    /// Test resource class for reflection-based operand resolution.
    /// </summary>
    private sealed class TestResource
    {
        public Guid OrgId { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid TenantId { get; set; }
        public decimal Amount { get; set; }
    }
}
