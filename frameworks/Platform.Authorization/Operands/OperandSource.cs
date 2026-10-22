namespace Platform.Authorization.Operands;

/// <summary>Nơi một operand quy tắc nhận giá trị của nó tại thời gian đánh giá.</summary>
public enum OperandSource
{
    /// <summary>Chủ thể được ủy quyền (<c>User.Id</c>).</summary>
    UserSubject,

    /// <summary>Cột phạm vi trên AspNetUsers (<c>User.TenantId</c>, <c>User.OrgId</c>).</summary>
    UserColumn,

    /// <summary>Mục nhập AspNetUserClaims của chủ thể.</summary>
    UserClaim,

    /// <summary>Cột phạm vi trên bảng resource (<c>Resource.TenantId</c>, <c>Resource.OrgId</c>).</summary>
    ResourceColumn,

    /// <summary>Attribute ABAC trên resource (DepartmentId, Amount, CreatedBy, …).</summary>
    Resource,
}
