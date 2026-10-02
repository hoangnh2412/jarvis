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
    private static readonly Guid Admin = Guid.Parse("10000000-0000-0000-0000-000000000001");
    private static readonly Guid TenantAdmin = Guid.Parse("10000000-0000-0000-0000-000000000002");
    private static readonly Guid OrgAdmin = Guid.Parse("10000000-0000-0000-0000-000000000003");
    private static readonly Guid Cfo = Guid.Parse("10000000-0000-0000-0000-000000000005");
    private static readonly Guid Employee = Guid.Parse("10000000-0000-0000-0000-000000000007");

    private static async Task<(InMemoryTestDbContext Ctx, RoleGovernanceValidator Validator)> CreateAsync()
    {
        var ctx = new InMemoryTestDbContext(new DbContextOptionsBuilder<InMemoryTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        await ctx.SeedWithBusinessAsync();
        return (ctx, new RoleGovernanceValidator(ctx));
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
        var (ctx, validator) = await CreateAsync();
        var actor = await AddUserAsync(ctx, OrgAdmin);

        Assert.True(await validator.CanManageRoleAsync(actor, Cfo));
        Assert.True(await validator.CanAssignRoleAsync(actor, Employee));
    }

    [Fact(DisplayName = "C5: strictly lower — OrgAdmin cannot manage OrgAdmin, TenantAdmin or Admin")]
    public async Task OrgAdmin_CannotManageEqualOrHigherRole()
    {
        var (ctx, validator) = await CreateAsync();
        var actor = await AddUserAsync(ctx, OrgAdmin);

        Assert.False(await validator.CanManageRoleAsync(actor, OrgAdmin));
        Assert.False(await validator.CanManageRoleAsync(actor, TenantAdmin));
        Assert.False(await validator.CanAssignRoleAsync(actor, Admin));
    }

    [Theory(DisplayName = "Phụ lục B: OrgAdmin creates role level 500 → ALLOW; 800/900 → DENY")]
    [InlineData(500, true)]
    [InlineData(799, true)]
    [InlineData(800, false)]
    [InlineData(900, false)]
    public async Task OrgAdmin_CreateRoleWithLevel(int level, bool expected)
    {
        var (ctx, validator) = await CreateAsync();
        var actor = await AddUserAsync(ctx, OrgAdmin);

        Assert.Equal(expected, await validator.CanCreateRoleWithLevelAsync(actor, level));
    }

    [Fact(DisplayName = "Multi-role actor uses the highest level")]
    public async Task MultiRoleActor_UsesMaxLevel()
    {
        var (ctx, validator) = await CreateAsync();
        var actor = await AddUserAsync(ctx, Employee, TenantAdmin);

        Assert.Equal(900, await validator.GetMaxRoleLevelAsync(actor));
        Assert.True(await validator.CanManageRoleAsync(actor, OrgAdmin));
    }

    [Fact(DisplayName = "Actor without any role cannot manage anything")]
    public async Task ActorWithoutRole_Denied()
    {
        var (ctx, validator) = await CreateAsync();
        var actor = Guid.NewGuid();
        var target = await AddUserAsync(ctx);

        Assert.Equal(0, await validator.GetMaxRoleLevelAsync(actor));
        Assert.False(await validator.CanManageRoleAsync(actor, Employee));
        Assert.False(await validator.CanCreateRoleWithLevelAsync(actor, 1));
        Assert.False(await validator.CanManageUserAsync(actor, target));
    }

    [Fact(DisplayName = "Unknown target role → DENY")]
    public async Task UnknownRole_Denied()
    {
        var (ctx, validator) = await CreateAsync();
        var actor = await AddUserAsync(ctx, Admin);

        Assert.False(await validator.CanManageRoleAsync(actor, Guid.NewGuid()));
    }

    [Fact(DisplayName = "S5: user management follows the target user's highest role")]
    public async Task ManageUser_ComparesHighestLevels()
    {
        var (ctx, validator) = await CreateAsync();
        var orgAdmin = await AddUserAsync(ctx, OrgAdmin);
        var admin = await AddUserAsync(ctx, Admin);
        var cfoAndTenantAdmin = await AddUserAsync(ctx, Cfo, TenantAdmin);
        var cfo = await AddUserAsync(ctx, Cfo);

        Assert.True(await validator.CanManageUserAsync(orgAdmin, cfo));
        Assert.False(await validator.CanManageUserAsync(orgAdmin, admin));
        Assert.False(await validator.CanManageUserAsync(orgAdmin, cfoAndTenantAdmin));
        Assert.False(await validator.CanManageUserAsync(orgAdmin, orgAdmin));
        Assert.True(await validator.CanManageUserAsync(admin, orgAdmin));
    }

    [Fact(DisplayName = "IsSystemRoleAsync reflects the seeded flag")]
    public async Task IsSystemRole_ReflectsSeed()
    {
        var (_, validator) = await CreateAsync();

        Assert.True(await validator.IsSystemRoleAsync(OrgAdmin));
        Assert.False(await validator.IsSystemRoleAsync(Cfo));
        Assert.False(await validator.IsSystemRoleAsync(Guid.NewGuid()));
    }
}
