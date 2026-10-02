namespace Platform.Authorization.Services;

using Platform.Authorization.Operands;

/// <summary>
/// Xác thực một quy tắc chính sách trước khi nó được lưu (ADR C7, D10, T9/T33–T39).
/// Operands đến từ <see cref="IOperandRegistry"/>; các toán tử là semantics của engine và không thay đổi.
/// </summary>
public sealed class OperandValidator
{
    private static readonly HashSet<string> OperatorWhitelist = new(StringComparer.Ordinal)
    {
        "==",
        "!=",
        "<",
        "<=",
        ">",
        ">=",
        "IN",
    };

    private readonly IOperandRegistry _registry;

    public OperandValidator(IOperandRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public ValidationResult ValidateRule(string leftOperand, string op, string rightOperand)
    {
        if (string.IsNullOrWhiteSpace(leftOperand))
            return ValidationResult.Fail("Operand bên trái không thể trống");

        if (!_registry.TryGet(leftOperand, out _))
            return ValidationResult.Fail($"Operand bên trái '{leftOperand}' không được đăng ký");

        if (string.IsNullOrWhiteSpace(op))
            return ValidationResult.Fail("Toán tử không thể trống");

        if (!IsValidOperator(op))
            return ValidationResult.Fail($"Toán tử '{op}' không được phép. Được phép: {string.Join(", ", OperatorWhitelist)}");

        if (string.IsNullOrWhiteSpace(rightOperand))
            return ValidationResult.Fail("Operand bên phải không thể trống");

        if (!_registry.TryGet(rightOperand, out var right))
            return ValidationResult.Fail($"Operand bên phải '{rightOperand}' không được đăng ký");

        if (op == "IN" && right.Kind != OperandValueKind.Collection)
            return ValidationResult.Fail($"Toán tử 'IN' yêu cầu operand collection. Nhận được: {rightOperand}");

        return ValidationResult.Success();
    }

    public bool IsValidOperand(string operand) => _registry.TryGet(operand, out _);

    public IEnumerable<string> GetWhitelistedOperands() => _registry.All.Select(d => d.Path);

    public static bool IsValidOperator(string op) => OperatorWhitelist.Contains(op);

    public static IEnumerable<string> GetWhitelistedOperators() => OperatorWhitelist;
}

/// <summary>
/// Kết quả của kiểm tra xác thực.
/// </summary>
public sealed class ValidationResult
{
    public bool IsValid { get; private set; }
    public string? ErrorMessage { get; private set; }

    public static ValidationResult Success() => new() { IsValid = true };
    public static ValidationResult Fail(string message) => new() { IsValid = false, ErrorMessage = message };
}
