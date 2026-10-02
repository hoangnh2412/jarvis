using Platform.Authorization.Operands;

namespace UnitTest.Authorization.Helpers;

/// <summary>
/// Platform defaults + the business operands a product registers (mirrors Lexora's
/// AuthorizationOperandsExtension, which platform tests cannot reference).
/// </summary>
internal static class TestOperands
{
    public static void Business(OperandCatalog operands) =>
        operands
            .AddUserClaim("DepartmentId")
            .AddUserClaim("ApproveLimit")
            .AddUserClaim("ManagedProjectIds", OperandValueKind.Collection)
            .AddUserClaim("ManagedDepartmentIds", OperandValueKind.Collection)
            .AddResource("DepartmentId")
            .AddResource("Amount")
            .AddResource("ProjectId")
            .AddResource("EmployeeId");

    public static OperandRegistry Registry() => OperandRegistry.Build(Business);
}
