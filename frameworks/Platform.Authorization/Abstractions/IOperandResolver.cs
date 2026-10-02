namespace Platform.Authorization.Abstractions;

/// <summary>
/// Giải quyết các giá trị operand trong quá trình đánh giá quy tắc chính sách. Operands phải được đăng ký trong
/// <see cref="Operands.IOperandRegistry"/>; bất cứ điều gì khác sẽ bị từ chối tại thời gian chạy (ADR D10).
/// </summary>
public interface IOperandResolver
{
    /// <summary>
    /// Giải quyết operand đã đăng ký ("User.OrgId", "Resource.Amount", …) thành giá trị của nó.
    /// Trả về vô hướng hoặc bộ sưu tập tùy thuộc vào định nghĩa operand.
    /// Ném <see cref="InvalidOperationException"/> nếu operand chưa được đăng ký.
    /// </summary>
    Task<object?> ResolveAsync(
        string operand,
        Guid userId,
        object? resource = null,
        CancellationToken cancellationToken = default);
}
