using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Platform.Authorization.EntityFramework.Persistence;
using Platform.Authorization.Services;
using Platform.Authorization.EntityFramework.Services;
using UnitTest.Authorization.Helpers;
using Xunit;

namespace UnitTest.Authorization;

/// <summary>
/// Role Governance (ADR C5, §5.4, Phụ lục B) over seeded system/business roles.
/// </summary>
public sealed class RoleGovernanceValidatorTests
{
    private sealed record SeededRoles(Guid Admin, Guid TenantAdmin, Guid OrgAdmin, Guid Cfo, Guid Employee);

    private static async Task<SeededRoles> LoadRoleIdsAsync(InMemoryTestDbContext ctx)
    {
        async Task<Guid> Id(string normalizedName) =>
            await ctx.Roles.Where(r => r.NormalizedName == normalizedName).Select(r => r.Id).SingleAsync();

        return new SeededRoles(
            await Id("ADMIN"),
            await Id("TENANT_ADMIN"),
            await Id("ORG_ADMIN"),
            await Id("CFO"),
            await Id("EMPLOYEE"));
    }

    private static async Task<(InMemoryTestDbContext Ctx, RoleGovernanceValidator Validator, SeededRoles Roles)> CreateAsync()
    {
        var ctx = new InMemoryTestDbContext(new DbContextOptionsBuilder<InMemoryTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        await ctx.SeedWithBusinessAsync();
        var roles = await LoadRoleIdsAsync(ctx);
        return (ctx, new RoleGovernanceValidator(ctx), roles);
    }

    private static async Task<Guid> AddUserAsync(InMemoryTestDbContext ctx, params Guid[] roleIds)
    {
        var userId = Guid.NewGuid();
        foreach (var roleId in roleIds)
            ctx.UserRoles.Add(new IdentityUserRole<Guid> { UserId = userId, RoleId = roleId });
        await ctx.SaveChangesAsync();
        return userId;
    }

    [Fact(DisplayName = "C5: OrgAdmin (800) manages CFO (650)")]
    public async Task OrgAdmin_CanManageLowerRole()
    {
        var (ctx, validator, roles) = await CreateAsync();
        var actor = await AddUserAsync(ctx, roles.OrgAdmin);

        Assert.True(await validator.CanManageRoleAsync(actor, roles.Cfo));
        Assert.True(await validator.CanAssignRoleAsync(actor, roles.Employee));
    }

    [Fact(DisplayName = "C5: strictly lower — OrgAdmin cannot manage OrgAdmin, TenantAdmin or Admin")]
    public async Task OrgAdmin_CannotManageEqualOrHigherRole()
    {
        var (ctx, validator, roles) = await CreateAsync();
        var actor = await AddUserAsync(ctx, roles.OrgAdmin);

        Assert.False(await validator.CanManageRoleAsync(actor, roles.OrgAdmin));
        Assert.False(await validator.CanManageRoleAsync(actor, roles.TenantAdmin));
        Assert.False(await validator.CanAssignRoleAsync(actor, roles.Admin));
    }

    [Theory(DisplayName = "Phụ lục B: OrgAdmin creates role level 500 → ALLOW; 800/900 → DENY")]
    [InlineData(500, true)]
    [InlineData(799, true)]
    [InlineData(800, false)]
    [InlineData(900, false)]
    public async Task OrgAdmin_CreateRoleWithLevel(int level, bool expected)
    {
        var (ctx, validator, roles) = await CreateAsync();
        var actor = await AddUserAsync(ctx, roles.OrgAdmin);

        Assert.Equal(expected, await validator.CanCreateRoleWithLevelAsync(actor, level));
    }

    [Fact(DisplayName = "Multi-role actor uses the highest level")]
    public async Task MultiRoleActor_UsesMaxLevel()
    {
        var (ctx, validator, roles) = await CreateAsync();
        var actor = await AddUserAsync(ctx, roles.Employee, roles.TenantAdmin);

        Assert.Equal(900, await validator.GetMaxRoleLevelAsync(actor));
        Assert.True(await validator.CanManageRoleAsync(actor, roles.OrgAdmin));
    }

    [Fact(DisplayName = "Actor without any role cannot manage anything")]
    public async Task ActorWithoutRole_Denied()
    {
        var (ctx, validator, roles) = await CreateAsync();
        var actor = Guid.NewGuid();
        var target = await AddUserAsync(ctx);

        Assert.Equal(0, await validator.GetMaxRoleLevelAsync(actor));
        Assert.False(await validator.CanManageRoleAsync(actor, roles.Employee));
        Assert.False(await validator.CanCreateRoleWithLevelAsync(actor, 1));
        Assert.False(await validator.CanManageUserAsync(actor, target));
    }

    [Fact(DisplayName = "Unknown target role → DENY")]
    public async Task UnknownRole_Denied()
    {
        var (ctx, validator, roles) = await CreateAsync();
        var actor = await AddUserAsync(ctx, roles.Admin);

        Assert.False(await validator.CanManageRoleAsync(actor, Guid.NewGuid()));
    }

    [Fact(DisplayName = "S5: user management follows the target user's highest role")]
    public async Task ManageUser_ComparesHighestLevels()
    {
        var (ctx, validator, roles) = await CreateAsync();
        var orgAdmin = await AddUserAsync(ctx, roles.OrgAdmin);
        var admin = await AddUserAsync(ctx, roles.Admin);
        var cfoAndTenantAdmin = await AddUserAsync(ctx, roles.Cfo, roles.TenantAdmin);
        var cfo = await AddUserAsync(ctx, roles.Cfo);

        Assert.True(await validator.CanManageUserAsync(orgAdmin, cfo));
        Assert.False(await validator.CanManageUserAsync(orgAdmin, admin));
        Assert.False(await validator.CanManageUserAsync(orgAdmin, cfoAndTenantAdmin));
        Assert.False(await validator.CanManageUserAsync(orgAdmin, orgAdmin));
        Assert.True(await validator.CanManageUserAsync(admin, orgAdmin));
    }

    [Fact(DisplayName = "IsSystemRoleAsync reflects the seeded flag")]
    public async Task IsSystemRole_ReflectsSeed()
    {
        var (_, validator, roles) = await CreateAsync();

        Assert.True(await validator.IsSystemRoleAsync(roles.OrgAdmin));
        Assert.False(await validator.IsSystemRoleAsync(roles.Cfo));
        Assert.False(await validator.IsSystemRoleAsync(Guid.NewGuid()));
    }
}
