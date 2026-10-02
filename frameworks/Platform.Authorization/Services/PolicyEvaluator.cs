namespace Platform.Authorization.Services;

using Platform.Authorization.Abstractions;
using Platform.Authorization.Domain;

/// <summary>
/// Đánh giá một chính sách duy nhất đối với người dùng và tài nguyên.
/// Đánh giá chính sách: tất cả các quy tắc được OR'd (bất kỳ quy tắc nào vượt qua → chính sách vượt qua).
/// </summary>
public sealed class PolicyEvaluator : IPolicyEvaluator
{
    private readonly IOperandResolver _operandResolver;

    public PolicyEvaluator(IOperandResolver operandResolver)
    {
        _operandResolver = operandResolver ?? throw new ArgumentNullException(nameof(operandResolver));
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
            var rightValue = await _operandResolver.ResolveAsync(rule.RightOperand, userId, resource, cancellationToken);

            return CompareValues(leftValue, rule.Operator, rightValue);
        }
        catch
        {
            // Nếu giải quyết operand thất bại, quy tắc không vượt qua
            return false;
        }
    }

    private bool CompareValues(object? left, string op, object? right)
    {
        // Một thuộc tính còn thiếu không bao giờ thỏa mãn một quy tắc; nếu không null == null sẽ cấp quyền truy cập
        // (ví dụ: người dùng không có OrgId vượt qua SameOrg trên tài nguyên không có OrgId).
        if (left is null || right is null)
        {
            return false;
        }

        return op.ToUpperInvariant() switch
        {
            "==" => Equals(left, right),
            "!=" => !Equals(left, right),
            "<" => Compare(left, right) < 0,
            "<=" => Compare(left, right) <= 0,
            ">" => Compare(left, right) > 0,
            ">=" => Compare(left, right) >= 0,
            "IN" => ContainsValue(right, left),
            _ => false
        };
    }

    private int Compare(object? left, object? right)
    {
        if (left == null && right == null) return 0;
        if (left == null) return -1;
        if (right == null) return 1;

        if (left is IComparable comparable)
        {
            return comparable.CompareTo(right);
        }

        return 0;
    }

    private bool ContainsValue(object? collection, object? value)
    {
        if (collection == null || value == null)
            return false;

        if (collection is System.Collections.IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                if (Equals(item, value))
                    return true;
            }
        }

        return false;
    }
}
