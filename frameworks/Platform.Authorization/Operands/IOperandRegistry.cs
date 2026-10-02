namespace Platform.Authorization.Operands;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Nguồn duy nhất có thẩm quyền cho các operand quy tắc được phép trắng, được xây dựng một lần tại thời gian khởi động từ
/// các mặc định của platform cộng với mọi đóng góp <c>AddPlatformAuthorizationOperands(...)</c>.
/// </summary>
public interface IOperandRegistry
{
    bool TryGet(string path, [NotNullWhen(true)] out OperandDefinition? definition);

    IReadOnlyCollection<OperandDefinition> All { get; }
}
