namespace Platform.Authorization.Operands;

/// <summary>Bộ đánh dấu DI cho một đóng góp khởi động cho danh sách operand.</summary>
public sealed class OperandContribution(Action<OperandCatalog> configure)
{
    internal Action<OperandCatalog> Configure { get; } = configure ?? throw new ArgumentNullException(nameof(configure));
}
