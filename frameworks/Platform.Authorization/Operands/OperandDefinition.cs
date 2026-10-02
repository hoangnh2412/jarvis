namespace Platform.Authorization.Operands;

/// <summary>Nơi một operand quy tắc nhận giá trị của nó tại thời gian đánh giá.</summary>
public enum OperandSource
{
    /// <summary>Chủ thể được ủy quyền (<c>User.Id</c>).</summary>
    UserSubject,

    /// <summary>Một cột của người dùng Danh tính (<c>User.TenantId</c>, <c>User.OrgId</c> — ADR Q3).</summary>
    UserColumn,

    /// <summary>Mục nhập AspNetUserClaims của chủ thể.</summary>
    UserClaim,

    /// <summary>Một thuộc tính công khai của tài nguyên đang được truy cập.</summary>
    Resource,
}

public enum OperandValueKind
{
    Scalar,

    /// <summary>Các giá trị được phân tách bằng dấu phẩy; chỉ được phép ở phía bên phải của <c>IN</c>.</summary>
    Collection,
}

/// <summary>
/// Một operand quy tắc được phép trắng (ADR C7, D10). Chỉ các operand đã đăng ký mới có thể được lưu trong quy tắc
/// hoặc được giải quyết tại thời gian chạy.
/// </summary>
public sealed record OperandDefinition(
    string Path,
    OperandSource Source,
    OperandValueKind Kind = OperandValueKind.Scalar,
    string? ClaimType = null)
{
    public const string UserPrefix = "User.";
    public const string ResourcePrefix = "Resource.";

    public string Attribute => Path[(Path.IndexOf('.') + 1)..];
}
