using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Platform.Authorization;
using Platform.Authorization.Operands;
using UnitTest.Authorization.Helpers;
using Xunit;

namespace UnitTest.Authorization;

/// <summary>
/// Operand registry (ADR C7, D10): platform registers only generic operands; business operands are contributed.
/// </summary>
public sealed class OperandRegistryTests
{
    [Theory(DisplayName = "Platform defaults: generic operands only")]
    [InlineData("User.Id", OperandSource.UserSubject)]
    [InlineData("User.TenantId", OperandSource.UserColumn)]
    [InlineData("User.OrgId", OperandSource.UserColumn)]
    [InlineData("Resource.TenantId", OperandSource.Resource)]
    [InlineData("Resource.OrgId", OperandSource.Resource)]
    [InlineData("Resource.CreatedBy", OperandSource.Resource)]
    public void Defaults_ContainGenericOperands(string path, OperandSource source)
    {
        var registry = OperandRegistry.Build();

        Assert.True(registry.TryGet(path, out var definition));
        Assert.Equal(source, definition.Source);
    }

    [Theory(DisplayName = "Platform defaults: no business operand (department, approve, project…)")]
    [InlineData("User.DepartmentId")]
    [InlineData("User.ApproveLimit")]
    [InlineData("User.ManagedProjectIds")]
    [InlineData("Resource.Amount")]
    [InlineData("Resource.ProjectId")]
    [InlineData("Resource.EmployeeId")]
    public void Defaults_HaveNoBusinessOperands(string path)
    {
        Assert.False(OperandRegistry.Build().TryGet(path, out _));
    }

    [Theory(DisplayName = "T9: sensitive or unknown operands are never registered")]
    [InlineData("User.Password")]
    [InlineData("User.PasswordHash")]
    [InlineData("Resource.Password")]
    [InlineData("Foo.Bar")]
    [InlineData("Admin.Bypass")]
    [InlineData("")]
    public void SensitiveOperands_NotRegistered(string path)
    {
        Assert.False(TestOperands.Registry().TryGet(path, out _));
    }

    [Fact(DisplayName = "Contribution adds business operands with their source and kind")]
    public void Contribution_AddsBusinessOperands()
    {
        var registry = TestOperands.Registry();

        Assert.True(registry.TryGet("User.ApproveLimit", out var limit));
        Assert.Equal(OperandSource.UserClaim, limit.Source);
        Assert.Equal("ApproveLimit", limit.ClaimType);

        Assert.True(registry.TryGet("User.ManagedProjectIds", out var managed));
        Assert.Equal(OperandValueKind.Collection, managed.Kind);

        Assert.True(registry.TryGet("Resource.Amount", out _));
    }

    [Fact(DisplayName = "Several modules can contribute; identical duplicates are accepted")]
    public void MultipleContributions_Merge()
    {
        var registry = OperandRegistry.Build(
            o => o.AddResource("Amount"),
            o => o.AddResource("Amount").AddUserClaim("DepartmentId"));

        Assert.True(registry.TryGet("Resource.Amount", out _));
        Assert.True(registry.TryGet("User.DepartmentId", out _));
    }

    [Fact(DisplayName = "Conflicting definitions for the same path fail at startup")]
    public void ConflictingDefinition_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => OperandRegistry.Build(
            o => o.AddUserClaim("ManagedProjectIds"),
            o => o.AddUserClaim("ManagedProjectIds", OperandValueKind.Collection)));
    }

    [Theory(DisplayName = "Invalid attribute names are rejected")]
    [InlineData("")]
    [InlineData("Profile.Id")]
    [InlineData("Approve Limit")]
    public void InvalidAttribute_Throws(string attribute)
    {
        Assert.Throws<ArgumentException>(() => OperandRegistry.Build(o => o.AddUserClaim(attribute)));
    }

    [Fact(DisplayName = "DI: AddPlatformAuthorizationOperands contributions reach the registry")]
    public void DependencyInjection_CollectsContributions()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.AddPlatformAuthorization();
        builder.AddPlatformAuthorizationOperands(TestOperands.Business);
        builder.AddPlatformAuthorizationOperands(o => o.AddResource("InvoiceNo"));

        using var host = builder.Build();
        var registry = host.Services.GetRequiredService<IOperandRegistry>();

        Assert.True(registry.TryGet("User.Id", out _));
        Assert.True(registry.TryGet("User.ApproveLimit", out _));
        Assert.True(registry.TryGet("Resource.InvoiceNo", out _));
    }
}
