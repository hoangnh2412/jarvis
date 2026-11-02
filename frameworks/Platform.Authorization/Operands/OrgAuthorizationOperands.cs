using Platform.Authorization.Operands;

namespace Platform.Authorization.Operands;

/// <summary>
/// Operand tổ chức đăng ký vào registry (ADR C7) — rule builder chỉ chọn trong danh sách này.
/// </summary>
public static class OrgAuthorizationOperands
{
    public static void Configure(OperandCatalog operands) =>
        operands
            .AddUserClaim("DepartmentId")
            .AddUserClaim("OrganizationalUnitId", valueType: OperandValueType.Guid)
            .AddUserClaim("PositionId", valueType: OperandValueType.Guid)
            .AddUserClaim("UnitType", valueType: OperandValueType.String)
            .AddUserClaim("LegalEntityId", valueType: OperandValueType.Guid)
            .AddUserClaim("OrganizationalUnitPath", valueType: OperandValueType.String)
            .AddUserClaim("ApproveLimit", valueType: OperandValueType.Decimal)
            .AddResource("DepartmentId")
            .AddResource("OrganizationalUnitId", valueType: OperandValueType.Guid)
            .AddResource("PositionId", valueType: OperandValueType.Guid)
            .AddResource("UnitType", valueType: OperandValueType.String)
            .AddResource("LegalEntityId", valueType: OperandValueType.Guid)
            .AddResource("OrganizationalUnitPath", valueType: OperandValueType.String)
            .AddResource("Amount", valueType: OperandValueType.Decimal);
}
