namespace Platform.Authorization.Services;

using System.Globalization;
using Platform.Authorization.Abstractions;
using Platform.Authorization.Domain;
using Platform.Authorization.Operands;

/// <summary>
/// Đánh giá một chính sách duy nhất đối với người dùng và tài nguyên.
/// Đánh giá chính sách: tất cả các quy tắc được OR'd (bất kỳ quy tắc nào vượt qua → chính sách vượt qua).
/// </summary>
public sealed class PolicyEvaluator : IPolicyEvaluator
{
    private readonly IOperandResolver _operandResolver;
    private readonly IOperandRegistry _operandRegistry;

    public PolicyEvaluator(IOperandResolver operandResolver, IOperandRegistry operandRegistry)
    {
        _operandResolver = operandResolver ?? throw new ArgumentNullException(nameof(operandResolver));
        _operandRegistry = operandRegistry ?? throw new ArgumentNullException(nameof(operandRegistry));
    }

    public async Task<bool> EvaluateAsync(
        Guid userId,
        Policy policy,
        object resource,
        CancellationToken cancellationToken = default)
    {
        if (policy.Rules.Count == 0)
        {
            // Chính sách không có quy tắc → không thể vượt qua
            return false;
        }

        // Đánh giá tất cả các quy tắc; trả về true nếu bất kỳ quy tắc nào vượt qua (semantics OR)
        foreach (var rule in policy.Rules.OrderBy(r => r.Order))
        {
            var passed = await EvaluateRuleAsync(userId, rule, resource, cancellationToken);
            if (passed)
            {
                return true; // Mạch ngắn: bất kỳ quy tắc nào vượt qua có nghĩa là chính sách vượt qua
            }
        }

        return false;
    }

    private async Task<bool> EvaluateRuleAsync(
        Guid userId,
        PolicyRule rule,
        object resource,
        CancellationToken cancellationToken)
    {
        try
        {
            var leftValue = await _operandResolver.ResolveAsync(rule.LeftOperand, userId, resource, cancellationToken);
            var rightValue = await ResolveRightValueAsync(rule, userId, resource, cancellationToken);

            return CompareValues(leftValue, rule.Operator, rightValue);
        }
        catch (InvalidOperationException)
        {
            // Operand chưa đăng ký hoặc không resolve được → rule không đạt.
            return false;
        }
    }

    private async Task<object?> ResolveRightValueAsync(
        PolicyRule rule,
        Guid userId,
        object resource,
        CancellationToken cancellationToken)
    {
        if (_operandRegistry.TryGet(rule.RightOperand, out _))
            return await _operandResolver.ResolveAsync(rule.RightOperand, userId, resource, cancellationToken);

        if (!_operandRegistry.TryGet(rule.LeftOperand, out var leftOperand))
            throw new InvalidOperationException($"Operand '{rule.LeftOperand}' không được đăng ký.");

        if (!RuleRightValueParser.TryParseLiteral(leftOperand, rule.RightOperand, out var literal, out var error))
            throw new InvalidOperationException(error ?? $"Giá trị vế phải '{rule.RightOperand}' không hợp lệ.");

        return literal;
    }

    private static bool CompareValues(object? left, string op, object? right)
    {
        // Thiếu thuộc tính không bao giờ đạt; nếu không thì null == null sẽ cho phép.
        if (left is null || right is null)
            return false;

        var compared = Compare(left, right);
        return op.ToUpperInvariant() switch
        {
            "==" => ValuesEqual(left, right),
            "!=" => !ValuesEqual(left, right),
            "<" => compared < 0,
            "<=" => compared <= 0,
            ">" => compared > 0,
            ">=" => compared >= 0,
            "IN" => ContainsValue(right, left),
            "STARTSWITH" => left.ToString()!.StartsWith(right.ToString()!, StringComparison.Ordinal),
            _ => false
        };
    }

    private static bool ValuesEqual(object left, object right)
    {
        if (TryToDecimal(left, out var leftNumber) && TryToDecimal(right, out var rightNumber))
            return leftNumber == rightNumber;

        return Equals(left, right);
    }

    /// <summary>
    /// So sánh được thì trả về âm/0/dương. Không so được (kiểu lệch, không IComparable) → null, rule không đạt.
    /// Hai số khác kiểu CLR (int/long/decimal) được đưa về decimal.
    /// </summary>
    private static int? Compare(object left, object right)
    {
        if (TryToDecimal(left, out var leftNumber) && TryToDecimal(right, out var rightNumber))
            return leftNumber.CompareTo(rightNumber);

        if (left.GetType() != right.GetType() || left is not IComparable comparable)
            return null;

        try
        {
            return comparable.CompareTo(right);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private static bool TryToDecimal(object value, out decimal number)
    {
        switch (value)
        {
            case byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal:
                number = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                return true;
            default:
                number = 0;
                return false;
        }
    }

    private static bool ContainsValue(object? collection, object? value)
    {
        if (collection == null || value == null || collection is string)
            return false;

        if (collection is System.Collections.IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                if (item is not null && ValuesEqual(item, value))
                    return true;
            }
        }

        return false;
    }
}
