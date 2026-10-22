namespace Platform.Authorization.Operands;

/// <summary>
/// Kiểu giá trị đã khai báo. <see cref="Infer"/> giữ cách đoán Guid → decimal → string.
/// Guid và String không được dùng với toán tử thứ tự (&lt;, &lt;=, &gt;, &gt;=).
/// </summary>
public enum OperandValueType
{
    Infer,
    Guid,
    Decimal,
    String,
}
