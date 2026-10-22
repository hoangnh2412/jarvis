namespace Platform.Authorization.Operands;

public enum OperandValueKind
{
    Scalar,

    /// <summary>Các giá trị được phân tách bằng dấu phẩy; chỉ được phép ở phía bên phải của <c>IN</c>.</summary>
    Collection,
}
