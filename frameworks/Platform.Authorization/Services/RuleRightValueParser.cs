namespace Platform.Authorization.Services;

using System.Globalization;
using Platform.Authorization.Operands;

/// <summary>
/// Vế phải rule: operand đã đăng ký hoặc hằng số (lưu dạng chuỗi trong <see cref="Domain.PolicyRule.RightOperand"/>).
/// </summary>
internal static class RuleRightValueParser
{
    public static bool AllowsLiteral(OperandDefinition leftOperand, string op)
    {
        if (string.Equals(op, "IN", StringComparison.OrdinalIgnoreCase))
            return false;

        if (IsOrderingOperator(op))
            return leftOperand.ValueType is OperandValueType.Decimal or OperandValueType.Infer;

        return true;
    }

    public static bool TryParseLiteral(
        OperandDefinition leftOperand,
        string literal,
        out object? value,
        out string? error)
    {
        value = null;
        error = null;

        if (string.IsNullOrWhiteSpace(literal))
        {
            error = "Giá trị vế phải không thể trống.";
            return false;
        }

        var trimmed = literal.Trim();

        switch (leftOperand.ValueType)
        {
            case OperandValueType.Decimal:
                if (decimal.TryParse(trimmed, NumberStyles.Number, CultureInfo.InvariantCulture, out var number))
                {
                    value = number;
                    return true;
                }

                error = "Vế phải phải là số (vd. 20000000).";
                return false;

            case OperandValueType.Guid:
                if (Guid.TryParse(trimmed, out var guid))
                {
                    value = guid;
                    return true;
                }

                error = "Vế phải phải là GUID hợp lệ.";
                return false;

            case OperandValueType.String:
                value = trimmed;
                return true;

            default:
                if (decimal.TryParse(trimmed, NumberStyles.Number, CultureInfo.InvariantCulture, out var inferredNumber))
                {
                    value = inferredNumber;
                    return true;
                }

                if (Guid.TryParse(trimmed, out var inferredGuid))
                {
                    value = inferredGuid;
                    return true;
                }

                value = trimmed;
                return true;
        }
    }

    private static bool IsOrderingOperator(string op) =>
        op is "<" or "<=" or ">" or ">=";
}
