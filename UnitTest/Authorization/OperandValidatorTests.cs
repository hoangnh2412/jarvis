using Platform.Authorization.Operands;
using Platform.Authorization.Services;
using UnitTest.Authorization.Helpers;
using Xunit;

namespace UnitTest.Authorization;

public sealed class OperandValidatorTests
{
    private readonly OperandValidator _validator = new(TestOperands.Registry());

    #region T9 - Operand/Operator Whitelist Validation

    [Fact(DisplayName = "T9a: Valid operand passes validation")]
    public void T9a_ValidOperandPasses()
    {
        var result = _validator.ValidateRule("Resource.OrgId", "==", "User.OrgId");

        Assert.True(result.IsValid);
    }

    [Fact(DisplayName = "T34: Invalid left operand rejected")]
    public void T34_InvalidLeftOperandRejected()
    {
        var result = _validator.ValidateRule("Resource.Password", "==", "User.OrgId");

        Assert.False(result.IsValid);
        Assert.Contains("Resource.Password", result.ErrorMessage);
    }

    [Fact(DisplayName = "T35: Malformed operand rejected")]
    public void T35_MalformedOperandRejected()
    {
        var result = _validator.ValidateRule("Env.Time", "==", "User.OrgId");

        Assert.False(result.IsValid);
    }

    [Fact(DisplayName = "T36: Invalid operator rejected")]
    public void T36_InvalidOperatorRejected()
    {
        var result = _validator.ValidateRule("Resource.OrgId", "LIKE", "User.OrgId");

        Assert.False(result.IsValid);
        Assert.Contains("LIKE", result.ErrorMessage);
    }

    [Fact(DisplayName = "T38: IN operator requires collection operand")]
    public void T38_InOperatorRequiresCollectionOperand()
    {
        var result = _validator.ValidateRule("Resource.OrgId", "IN", "User.OrgId");

        Assert.False(result.IsValid);
        Assert.Contains("IN", result.ErrorMessage);
    }

    [Fact(DisplayName = "T38b: IN operator passes with collection operand")]
    public void T38b_InOperatorWithCollectionOperandPasses()
    {
        var result = _validator.ValidateRule("Resource.ProjectId", "IN", "User.ManagedProjectIds");

        Assert.True(result.IsValid);
    }

    [Fact(DisplayName = "T38c: collection is decided by registration, not by an 'Ids' suffix")]
    public void T38c_CollectionDecidedByRegistration()
    {
        var validator = new OperandValidator(OperandRegistry.Build(o => o
            .AddUserClaim("RegionIds")
            .AddUserClaim("Regions", OperandValueKind.Collection)
            .AddResource("Region")));

        Assert.False(validator.ValidateRule("Resource.Region", "IN", "User.RegionIds").IsValid);
        Assert.True(validator.ValidateRule("Resource.Region", "IN", "User.Regions").IsValid);
    }

    [Fact(DisplayName = "Business operand is rejected when the product did not register it")]
    public void BusinessOperand_RejectedWithoutRegistration()
    {
        var platformOnly = new OperandValidator(OperandRegistry.Build());

        Assert.False(platformOnly.ValidateRule("Resource.Amount", "<=", "User.ApproveLimit").IsValid);
    }

    [Fact(DisplayName = "T9b: Get whitelisted operands")]
    public void T9b_GetWhitelistedOperands()
    {
        var operands = _validator.GetWhitelistedOperands().ToList();

        Assert.Contains("User.Id", operands);
        Assert.Contains("Resource.OrgId", operands);
        Assert.Contains("User.ApproveLimit", operands);
    }

    [Fact(DisplayName = "T9c: Get whitelisted operators")]
    public void T9c_GetWhitelistedOperators()
    {
        var operators = OperandValidator.GetWhitelistedOperators().ToList();

        Assert.Contains("==", operators);
        Assert.Contains("IN", operators);
        Assert.DoesNotContain("LIKE", operators);
    }

    #endregion
}
