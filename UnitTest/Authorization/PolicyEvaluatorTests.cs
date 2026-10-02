using Moq;
using Platform.Authorization.Abstractions;
using Platform.Authorization.Domain;
using Platform.Authorization.Services;
using Xunit;

namespace UnitTest.Authorization;

public sealed class PolicyEvaluatorTests
{
    private readonly Mock<IOperandResolver> _resolverMock;
    private readonly PolicyEvaluator _evaluator;

    public PolicyEvaluatorTests()
    {
        _resolverMock = new Mock<IOperandResolver>();
        _evaluator = new PolicyEvaluator(_resolverMock.Object);
    }

    #region T60-T71 - Policy Evaluation Matrix

    [Fact(DisplayName = "T60: P_SAME_TENANT - passing case")]
    public async Task T60_SameTenantPasses()
    {
        // Arrange
        var policy = CreateTestPolicy("P_SAME_TENANT", "Same Tenant");
        var rule = CreateTestRule(policy.Id, 1, "Resource.TenantId", "==", "User.TenantId");
        policy.Rules.Add(rule);

        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var resource = new { TenantId = tenantId };

        _resolverMock
            .Setup(r => r.ResolveAsync("Resource.TenantId", userId, resource, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenantId);

        _resolverMock
            .Setup(r => r.ResolveAsync("User.TenantId", userId, resource, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenantId);

        // Act
        var result = await _evaluator.EvaluateAsync(userId, policy, resource);

        // Assert
        Assert.True(result);
    }

    [Fact(DisplayName = "T61: P_SAME_TENANT - failing case")]
    public async Task T61_SameTenantFails()
    {
        // Arrange
        var policy = CreateTestPolicy("P_SAME_TENANT", "Same Tenant");
        var rule = CreateTestRule(policy.Id, 1, "Resource.TenantId", "==", "User.TenantId");
        policy.Rules.Add(rule);

        var userId = Guid.NewGuid();
        var userTenantId = Guid.NewGuid();
        var resourceTenantId = Guid.NewGuid();
        var resource = new { TenantId = resourceTenantId };

        _resolverMock
            .Setup(r => r.ResolveAsync("Resource.TenantId", userId, resource, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resourceTenantId);

        _resolverMock
            .Setup(r => r.ResolveAsync("User.TenantId", userId, resource, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userTenantId);

        // Act
        var result = await _evaluator.EvaluateAsync(userId, policy, resource);

        // Assert
        Assert.False(result);
    }

    [Fact(DisplayName = "T74: P_OWNER_OR_SAME_DEPT - owner passes (OR semantics, first rule)")]
    public async Task T74_OwnerOrSameDept_OwnerPasses()
    {
        // Arrange - multi-rule policy (OR semantics)
        var policy = CreateTestPolicy("P_OWNER_OR_SAME_DEPT", "Owner Or Same Dept");
        var rule1 = CreateTestRule(policy.Id, 1, "Resource.CreatedBy", "==", "User.Id");
        var rule2 = CreateTestRule(policy.Id, 2, "Resource.DepartmentId", "==", "User.DepartmentId");
        policy.Rules.Add(rule1);
        policy.Rules.Add(rule2);

        var userId = Guid.NewGuid();
        var otherDeptId = Guid.NewGuid();
        var resource = new { CreatedBy = userId, DepartmentId = otherDeptId };

        // Rule 1 passes: CreatedBy == UserId
        _resolverMock
            .Setup(r => r.ResolveAsync("Resource.CreatedBy", userId, resource, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);

        _resolverMock
            .Setup(r => r.ResolveAsync("User.Id", userId, resource, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);

        // Act
        var result = await _evaluator.EvaluateAsync(userId, policy, resource);

        // Assert
        Assert.True(result, "Policy should pass when first rule (owner) passes");
    }

    [Fact(DisplayName = "T75: P_OWNER_OR_SAME_DEPT - same dept passes (OR semantics, second rule)")]
    public async Task T75_OwnerOrSameDept_SameDeptPasses()
    {
        // Arrange
        var policy = CreateTestPolicy("P_OWNER_OR_SAME_DEPT", "Owner Or Same Dept");
        var rule1 = CreateTestRule(policy.Id, 1, "Resource.CreatedBy", "==", "User.Id");
        var rule2 = CreateTestRule(policy.Id, 2, "Resource.DepartmentId", "==", "User.DepartmentId");
        policy.Rules.Add(rule1);
        policy.Rules.Add(rule2);

        var userId = Guid.NewGuid();
        var deptId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var resource = new { CreatedBy = otherUserId, DepartmentId = deptId };

        // Rule 1 fails
        _resolverMock
            .Setup(r => r.ResolveAsync("Resource.CreatedBy", userId, resource, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherUserId);

        _resolverMock
            .Setup(r => r.ResolveAsync("User.Id", userId, resource, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);

        // Rule 2 passes
        _resolverMock
            .Setup(r => r.ResolveAsync("Resource.DepartmentId", userId, resource, It.IsAny<CancellationToken>()))
            .ReturnsAsync(deptId);

        _resolverMock
            .Setup(r => r.ResolveAsync("User.DepartmentId", userId, resource, It.IsAny<CancellationToken>()))
            .ReturnsAsync(deptId);

        // Act
        var result = await _evaluator.EvaluateAsync(userId, policy, resource);

        // Assert
        Assert.True(result, "Policy should pass when second rule (same dept) passes");
    }

    [Fact(DisplayName = "T76: P_OWNER_OR_SAME_DEPT - both fail")]
    public async Task T76_OwnerOrSameDept_BothFail()
    {
        // Arrange
        var policy = CreateTestPolicy("P_OWNER_OR_SAME_DEPT", "Owner Or Same Dept");
        var rule1 = CreateTestRule(policy.Id, 1, "Resource.CreatedBy", "==", "User.Id");
        var rule2 = CreateTestRule(policy.Id, 2, "Resource.DepartmentId", "==", "User.DepartmentId");
        policy.Rules.Add(rule1);
        policy.Rules.Add(rule2);

        var userId = Guid.NewGuid();
        var userDeptId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var otherDeptId = Guid.NewGuid();
        var resource = new { CreatedBy = otherUserId, DepartmentId = otherDeptId };

        // Both rules fail
        _resolverMock
            .Setup(r => r.ResolveAsync("Resource.CreatedBy", userId, resource, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherUserId);
        _resolverMock
            .Setup(r => r.ResolveAsync("User.Id", userId, resource, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);
        _resolverMock
            .Setup(r => r.ResolveAsync("Resource.DepartmentId", userId, resource, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherDeptId);
        _resolverMock
            .Setup(r => r.ResolveAsync("User.DepartmentId", userId, resource, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userDeptId);

        // Act
        var result = await _evaluator.EvaluateAsync(userId, policy, resource);

        // Assert
        Assert.False(result, "Policy should fail when all rules fail");
    }

    #endregion

    #region T77-T80 - Complex Multi-Rule Scenarios

    [Fact(DisplayName = "T77: Policy with first rule passing (OR semantics)")]
    public async Task T77_FirstRulePasses()
    {
        // Arrange - single policy with 2 rules (OR semantics)
        // Rules in same policy → any rule passes = policy passes
        var policy = CreateTestPolicy("P_OWNER_OR_LIMIT", "Owner Or Limit");
        var rule1 = CreateTestRule(policy.Id, 1, "Resource.CreatedBy", "==", "User.Id");
        var rule2 = CreateTestRule(policy.Id, 2, "Resource.Amount", "<=", "User.ApproveLimit");
        policy.Rules.Add(rule1);
        policy.Rules.Add(rule2);

        var userId = Guid.NewGuid();
        var resource = new { CreatedBy = userId, Amount = 100m };

        _resolverMock
            .Setup(r => r.ResolveAsync("Resource.CreatedBy", userId, resource, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);
        _resolverMock
            .Setup(r => r.ResolveAsync("User.Id", userId, resource, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);

        // Act
        var result = await _evaluator.EvaluateAsync(userId, policy, resource);

        // Assert - passes because first rule passes (OR semantics)
        Assert.True(result);
    }

    [Fact(DisplayName = "T78: Policy with first rule failing, second passing (OR semantics)")]
    public async Task T78_SecondRulePasses()
    {
        // Arrange
        var policy = CreateTestPolicy("P_OWNER_OR_LIMIT", "Owner Or Limit");
        var rule1 = CreateTestRule(policy.Id, 1, "Resource.CreatedBy", "==", "User.Id");
        var rule2 = CreateTestRule(policy.Id, 2, "Resource.Amount", "<=", "User.ApproveLimit");
        policy.Rules.Add(rule1);
        policy.Rules.Add(rule2);

        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var resource = new { CreatedBy = otherUserId, Amount = 8m };

        _resolverMock
            .Setup(r => r.ResolveAsync("Resource.CreatedBy", userId, resource, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherUserId);
        _resolverMock
            .Setup(r => r.ResolveAsync("User.Id", userId, resource, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);
        _resolverMock
            .Setup(r => r.ResolveAsync("Resource.Amount", userId, resource, It.IsAny<CancellationToken>()))
            .ReturnsAsync(8m);
        _resolverMock
            .Setup(r => r.ResolveAsync("User.ApproveLimit", userId, resource, It.IsAny<CancellationToken>()))
            .ReturnsAsync(10m);

        // Act
        var result = await _evaluator.EvaluateAsync(userId, policy, resource);

        // Assert - passes because second rule passes (OR semantics, short-circuit)
        Assert.True(result);
    }

    #endregion

    #region T80+ - Edge Cases

    [Fact(DisplayName = "T80: Policy with no rules cannot pass")]
    public async Task T80_EmptyPolicyFails()
    {
        // Arrange
        var policy = CreateTestPolicy("P_EMPTY", "Empty Policy");
        // No rules added
        var userId = Guid.NewGuid();
        var resource = new { };

        // Act
        var result = await _evaluator.EvaluateAsync(userId, policy, resource);

        // Assert
        Assert.False(result);
    }

    [Fact(DisplayName = "T81: Operand resolution exception → rule fails (doesn't throw)")]
    public async Task T81_ResolutionExceptionHandled()
    {
        // Arrange
        var policy = CreateTestPolicy("P_TEST", "Test");
        var rule = CreateTestRule(policy.Id, 1, "Resource.OrgId", "==", "User.OrgId");
        policy.Rules.Add(rule);

        var userId = Guid.NewGuid();
        var resource = new { };

        _resolverMock
            .Setup(r => r.ResolveAsync(It.IsAny<string>(), userId, resource, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Resolution failed"));

        // Act
        var result = await _evaluator.EvaluateAsync(userId, policy, resource);

        // Assert - should not throw, rule just fails
        Assert.False(result);
    }

    #endregion

    #region Helpers

    private static Policy CreateTestPolicy(string code, string name)
    {
        return new Policy
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            IsSystemPolicy = false,
            Rules = new List<PolicyRule>()
        };
    }

    private static PolicyRule CreateTestRule(
        Guid policyId,
        int order,
        string leftOperand,
        string op,
        string rightOperand)
    {
        return new PolicyRule
        {
            Id = Guid.NewGuid(),
            PolicyId = policyId,
            Order = order,
            LeftOperand = leftOperand,
            Operator = op,
            RightOperand = rightOperand
        };
    }

    #endregion
}
