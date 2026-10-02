using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Platform.Authorization;
using Platform.Authorization.Domain;
using Platform.Authorization.EntityFramework;
using Platform.Authorization.EntityFramework.Persistence;
using Platform.Authorization.Operands;
using Platform.Authorization.Services;
using Platform.Authorization.EntityFramework.Services;
using UnitTest.Authorization.Helpers;
using Xunit;

namespace UnitTest.Authorization;

/// <summary>
/// Test suite for AuthorizationSeeder (T20–T25, ADR V5).
/// Platform seeds admin roles + generic policies; business roles/policies come from product contributions.
/// </summary>
public sealed class AuthorizationSeederTests
{
    private static readonly string[] PlatformPolicyCodes = ["P_SAME_TENANT", "P_SAME_ORG", "P_OWNER_ONLY"];
    private static readonly string[] BusinessPolicyCodes = ["P_SAME_DEPARTMENT", "P_APPROVE_LIMIT", "P_MANAGED_PROJECT", "P_OWNER_OR_SAME_DEPT"];

    private InMemoryTestDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<InMemoryTestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new InMemoryTestDbContext(options);
    }

    #region T20 - Seed Idempotent Policies

    [Fact(DisplayName = "T20: Seed idempotent — no duplicate Policy.Code after 2nd seed")]
    public async Task T20_SeedIdempotentPolicies_NoDuplicate()
    {
        // Arrange
        var context = CreateInMemoryDbContext();

        // Act — seed first time
        await context.SeedWithBusinessAsync();
        var firstCount = await context.Policies.CountAsync();

        // Act — seed second time (should be idempotent)
        await context.SeedWithBusinessAsync();
        var secondCount = await context.Policies.CountAsync();

        // Assert
        Assert.Equal(firstCount, secondCount);
        Assert.Equal(7, secondCount); // 3 platform + 4 business
        Assert.Equal(7, await context.Roles.CountAsync()); // 3 platform + 4 business
    }

    [Fact(DisplayName = "T21: Seed idempotent — PolicyRules exist per Policy with correct Order")]
    public async Task T21_SeedIdempotentPolicyRules_CorrectOrder()
    {
        // Arrange
        var context = CreateInMemoryDbContext();

        // Act — seed
        await context.SeedWithBusinessAsync();

        // Assert
        var policies = await context.Policies
            .Include(p => p.Rules)
            .ToListAsync();

        // P_SAME_TENANT: 1 rule, Order=1
        var sameTenant = policies.FirstOrDefault(p => p.Code == "P_SAME_TENANT");
        Assert.NotNull(sameTenant);
        Assert.Single(sameTenant!.Rules);
        Assert.Equal(1, sameTenant.Rules.First().Order);

        // P_OWNER_OR_SAME_DEPT: 2 rules, Order=1,2
        var ownerOrDept = policies.FirstOrDefault(p => p.Code == "P_OWNER_OR_SAME_DEPT");
        Assert.NotNull(ownerOrDept);
        Assert.Equal(2, ownerOrDept!.Rules.Count);
        Assert.Equal(new[] { 1, 2 }, ownerOrDept.Rules.OrderBy(r => r.Order).Select(r => r.Order));
    }

    [Fact(DisplayName = "T22: IPolicyProvider.GetPolicyByCodeAsync('P_SAME_ORG') returns policy + rule")]
    public async Task T22_PolicyProvider_GetPolicyByCodeAsync_ReturnsPolicy()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        await context.SeedAsync();

        var provider = new PolicyProvider(context);

        // Act
        var policy = await provider.GetPolicyByCodeAsync("P_SAME_ORG");

        // Assert
        Assert.NotNull(policy);
        Assert.Equal("P_SAME_ORG", policy!.Code);
        Assert.Equal("Same Organization", policy.Name);
        Assert.Single(policy.Rules);
        Assert.Equal("Resource.OrgId", policy.Rules.First().LeftOperand);
        Assert.Equal("==", policy.Rules.First().Operator);
        Assert.Equal("User.OrgId", policy.Rules.First().RightOperand);
    }

    [Fact(DisplayName = "T23: IPolicyProvider.GetPolicyByCodeAsync('P_UNKNOWN') returns null")]
    public async Task T23_PolicyProvider_GetPolicyByCodeAsync_UnknownPolicyReturnsNull()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        await context.SeedAsync();

        var provider = new PolicyProvider(context);

        // Act
        var policy = await provider.GetPolicyByCodeAsync("P_UNKNOWN");

        // Assert
        Assert.Null(policy);
    }

    [Fact(DisplayName = "T24: List system policies returns all seeded policies")]
    public async Task T24_ListSystemPolicies_ReturnsAllSeeded()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        await context.SeedWithBusinessAsync();

        var provider = new PolicyProvider(context);

        // Act
        var policies = await provider.GetSystemPoliciesAsync();

        // Assert
        var codes = policies.Select(p => p.Code).ToHashSet();
        Assert.Equal(PlatformPolicyCodes.Concat(BusinessPolicyCodes).ToHashSet(), codes);
    }

    [Fact(DisplayName = "T25: Policy kèm Rules theo Order")]
    public async Task T25_GetPolicy_IncludesRulesOrderedByOrder()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        await context.SeedWithBusinessAsync();

        var provider = new PolicyProvider(context);

        // Act
        var policy = await provider.GetPolicyByCodeAsync("P_OWNER_OR_SAME_DEPT");

        // Assert
        Assert.NotNull(policy);
        Assert.Equal(2, policy!.Rules.Count);
        var orderedRules = policy.Rules.OrderBy(r => r.Order).ToList();
        Assert.Equal(1, orderedRules[0].Order);
        Assert.Equal(2, orderedRules[1].Order);
        Assert.Equal("Resource.CreatedBy", orderedRules[0].LeftOperand); // Order 1 = Owner
        Assert.Equal("Resource.DepartmentId", orderedRules[1].LeftOperand); // Order 2 = SameDept
    }

    #endregion

    #region V5 - Platform seed is generic; business seed is contributed

    [Fact(DisplayName = "V5: platform seed only — admin roles + generic policies, no business policy")]
    public async Task V5_PlatformSeed_IsGenericOnly()
    {
        var context = CreateInMemoryDbContext();

        await context.SeedAsync();

        var roles = await context.Roles.Select(r => r.Name).ToListAsync();
        var codes = await context.Policies.Select(p => p.Code).ToListAsync();
        Assert.Equal(new[] { "ADMIN", "ORG_ADMIN", "TENANT_ADMIN" }, roles.OrderBy(r => r));
        Assert.Equal(PlatformPolicyCodes.OrderBy(c => c), codes.OrderBy(c => c));
        Assert.True(await context.Roles.AllAsync(r => r.IsSystemRole));
    }

    [Fact(DisplayName = "V5: platform policies use only platform operands")]
    public void V5_PlatformPolicies_ValidAgainstPlatformRegistry()
    {
        var validator = new OperandValidator(OperandRegistry.Build());

        var rules = AuthorizationSeeder.BuildCatalog().Policies.SelectMany(p => p.Rules);

        Assert.All(rules, r => Assert.True(validator.ValidateRule(r.LeftOperand, r.Operator, r.RightOperand).IsValid));
    }

    [Fact(DisplayName = "V5: business seed without operand registration fails before writing")]
    public async Task V5_BusinessSeed_RejectedWithoutOperandRegistration()
    {
        var context = CreateInMemoryDbContext();
        var platformOnly = new OperandValidator(OperandRegistry.Build());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => context.SeedAsync(AuthorizationSeeder.BuildCatalog(TestSeeds.Business), platformOnly));

        Assert.Contains("P_APPROVE_LIMIT#1", ex.Message);
        Assert.Contains("Resource.Amount", ex.Message);
        Assert.Equal(0, await context.Policies.CountAsync());
        Assert.Equal(0, await context.Roles.CountAsync());
    }

    [Fact(DisplayName = "V5: seeding inserts missing items only and keeps existing edits")]
    public async Task V5_Seed_InsertsMissingOnly_KeepsExisting()
    {
        var context = CreateInMemoryDbContext();
        await context.SeedAsync();

        var sameOrg = await context.Policies.SingleAsync(p => p.Code == "P_SAME_ORG");
        sameOrg.Name = "Edited by admin";
        await context.SaveChangesAsync();

        // A product added later contributes business seed on an existing database.
        await context.SeedWithBusinessAsync();

        Assert.Equal("Edited by admin", (await context.Policies.SingleAsync(p => p.Code == "P_SAME_ORG")).Name);
        Assert.Equal(7, await context.Policies.CountAsync());
        Assert.Equal(7, await context.Roles.CountAsync());
    }

    [Fact(DisplayName = "V5: duplicate role name or policy code in the seed catalog is rejected")]
    public void V5_DuplicateSeedItem_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => AuthorizationSeeder.BuildCatalog(
            seed => seed.AddPolicy(new Policy { Id = Guid.NewGuid(), Code = "P_SAME_ORG", Name = "Clash" })));
        Assert.Throws<InvalidOperationException>(() => AuthorizationSeeder.BuildCatalog(
            seed => seed.AddRole(new Platform.Authentication.Identity.Domain.Role { Id = Guid.NewGuid(), Name = "admin" })));
    }

    [Fact(DisplayName = "V5: AuthorizationDataSeeder combines DI contributions and validates with the registry")]
    public async Task V5_DataSeeder_UsesDiContributions()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.AddPlatformAuthorization();
        builder.AddPlatformAuthorizationEntityFramework();
        builder.AddPlatformAuthorizationOperands(TestOperands.Business);
        builder.AddPlatformAuthorizationSeed(TestSeeds.Business);
        var databaseName = Guid.NewGuid().ToString();
        builder.Services.AddDbContext<InMemoryTestDbContext>(o => o.UseInMemoryDatabase(databaseName));
        builder.Services.AddScoped<IdentityDbContextBase>(sp => sp.GetRequiredService<InMemoryTestDbContext>());

        using var host = builder.Build();
        using var scope = host.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<AuthorizationDataSeeder>().SeedAsync();

        var context = scope.ServiceProvider.GetRequiredService<InMemoryTestDbContext>();
        Assert.Equal(7, await context.Policies.CountAsync());
        Assert.Equal(7, await context.Roles.CountAsync());
    }

    #endregion

    #region T1b - Smoke Test (Seed loads without error)

    [Fact(DisplayName = "T1b: Seed Policies + PolicyRules load without error")]
    public async Task T1b_SeedLoadsSuccessfully()
    {
        // Arrange
        var context = CreateInMemoryDbContext();

        // Act & Assert (no exception thrown)
        await context.SeedWithBusinessAsync();

        var policyCount = await context.Policies.CountAsync();
        var ruleCount = await context.PolicyRules.CountAsync();

        Assert.Equal(7, policyCount);
        Assert.Equal(8, ruleCount); // 6 policies × 1 rule + P_OWNER_OR_SAME_DEPT × 2 rules = 8
    }

    #endregion
}
