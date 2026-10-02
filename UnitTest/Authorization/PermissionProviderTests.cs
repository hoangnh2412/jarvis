using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Platform.Authorization.Services;
using Platform.Authorization.EntityFramework.Services;
using UnitTest.Authorization.Helpers;
using Xunit;

namespace UnitTest.Authorization;

/// <summary>
/// Real PermissionProvider over InMemory store — wildcard semantics (ADR D8/Q2).
/// </summary>
public sealed class PermissionProviderTests
{
    private static InMemoryTestDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<InMemoryTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static async Task<Guid> AddUserWithRoleAsync(InMemoryTestDbContext ctx, params string[] permissions)
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        ctx.UserRoles.Add(new IdentityUserRole<Guid> { UserId = userId, RoleId = roleId });
        foreach (var permission in permissions)
        {
            ctx.RoleClaims.Add(new IdentityRoleClaim<Guid> { RoleId = roleId, ClaimType = "Permission", ClaimValue = permission });
        }

        await ctx.SaveChangesAsync();
        return userId;
    }

    [Theory(DisplayName = "S2: asking for a wildcard is denied when the user does not hold one")]
    [InlineData("*")]
    [InlineData("System.All")]
    public async Task RequestedWildcard_WithoutWildcardGrant_ReturnsFalse(string requested)
    {
        await using var ctx = CreateContext();
        var userId = await AddUserWithRoleAsync(ctx, "Expense.View");
        var provider = new PermissionProvider(ctx);

        Assert.False(await provider.HasPermissionAsync(userId, requested));
    }

    [Fact(DisplayName = "S2: user without any role cannot pass a wildcard request")]
    public async Task RequestedWildcard_UserWithoutRoles_ReturnsFalse()
    {
        await using var ctx = CreateContext();
        var provider = new PermissionProvider(ctx);

        Assert.False(await provider.HasPermissionAsync(Guid.NewGuid(), "*"));
    }

    [Theory(DisplayName = "S2: a held wildcard grants any specific permission")]
    [InlineData("*")]
    [InlineData("System.All")]
    public async Task HeldWildcard_GrantsSpecificPermission(string wildcard)
    {
        await using var ctx = CreateContext();
        var userId = await AddUserWithRoleAsync(ctx, wildcard);
        var provider = new PermissionProvider(ctx);

        Assert.True(await provider.HasPermissionAsync(userId, "Expense.Approve"));
        Assert.True(await provider.HasPermissionAsync(userId, "*"));
    }

    [Fact(DisplayName = "Specific permission: granted only when held")]
    public async Task SpecificPermission_GrantedOnlyWhenHeld()
    {
        await using var ctx = CreateContext();
        var userId = await AddUserWithRoleAsync(ctx, "Expense.View");
        var provider = new PermissionProvider(ctx);

        Assert.True(await provider.HasPermissionAsync(userId, "Expense.View"));
        Assert.False(await provider.HasPermissionAsync(userId, "Expense.Approve"));
    }

    [Fact(DisplayName = "GetPermissionsByRoleAsync groups permissions per role")]
    public async Task GetPermissionsByRole_GroupsPerRole()
    {
        await using var ctx = CreateContext();
        var userId = Guid.NewGuid();
        var role1 = Guid.NewGuid();
        var role2 = Guid.NewGuid();
        ctx.UserRoles.AddRange(
            new IdentityUserRole<Guid> { UserId = userId, RoleId = role1 },
            new IdentityUserRole<Guid> { UserId = userId, RoleId = role2 });
        ctx.RoleClaims.AddRange(
            new IdentityRoleClaim<Guid> { RoleId = role1, ClaimType = "Permission", ClaimValue = "Expense.View" },
            new IdentityRoleClaim<Guid> { RoleId = role1, ClaimType = "Policy", ClaimValue = "P_SAME_ORG" },
            new IdentityRoleClaim<Guid> { RoleId = role2, ClaimType = "Permission", ClaimValue = "Project.View" },
            new IdentityRoleClaim<Guid> { RoleId = Guid.NewGuid(), ClaimType = "Permission", ClaimValue = "Other.Role" });
        await ctx.SaveChangesAsync();

        var result = await new PermissionProvider(ctx).GetPermissionsByRoleAsync(userId);

        Assert.Equal(2, result.Count);
        Assert.Equal(new[] { "Expense.View" }, result[role1]);
        Assert.Equal(new[] { "Project.View" }, result[role2]);
    }
}
