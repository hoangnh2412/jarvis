namespace Platform.Authorization.Operands;

/// <summary>
/// Một operand quy tắc được phép trắng (ADR C7, D10). Chỉ các operand đã đăng ký mới có thể được lưu trong quy tắc
/// hoặc được giải quyết tại thời gian chạy.
/// </summary>
public sealed record OperandDefinition(
    string Path,
    OperandSource Source,
    OperandValueKind Kind = OperandValueKind.Scalar,
    string? ClaimType = null,
    OperandValueType ValueType = OperandValueType.Infer)
{
    public const string UserPrefix = "User.";
    public const string ResourcePrefix = "Resource.";

    public string Attribute => Path[(Path.IndexOf('.') + 1)..];
}
