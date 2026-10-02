namespace Platform.Authorization.Services;

using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Platform.Authorization.Abstractions;
using Platform.Authorization.Domain;
using Platform.Authorization.Operands;

/// <summary>
/// Áp dụng phạm vi dữ liệu (ADR §5.11, T10) bằng cách dịch policy của user thành biểu thức
/// <c>Where</c> để provider (EF Core) đẩy xuống SQL — không tải toàn bộ rồi lọc.
/// Ngữ nghĩa trùng <see cref="AuthorizationEngine"/>: chỉ role cấp <c>permission</c> mới tạo đường dẫn;
/// policy AND trong một role, OR giữa các role (Q8); rule OR trong một policy (Q5); wildcard không lọc (Q7);
/// thiếu thuộc tính / operand chưa đăng ký / policy không resolve được → rule (hoặc đường dẫn role) không đạt.
/// So sánh chỉ đạt khi giá trị user cùng kiểu với thuộc tính resource (như <see cref="PolicyEvaluator"/>).
/// </summary>
public sealed class DataScopeFilter : IDataScopeFilter
{
    private static readonly Expression True = Expression.Constant(true);
    private static readonly Expression False = Expression.Constant(false);

    private readonly IPermissionProvider _permissionProvider;
    private readonly IPolicyProvider _policyProvider;
    private readonly IPolicyEvaluator _policyEvaluator;
    private readonly IOperandResolver _operandResolver;
    private readonly IOperandRegistry _operands;

    public DataScopeFilter(
        IPermissionProvider permissionProvider,
        IPolicyProvider policyProvider,
        IPolicyEvaluator policyEvaluator,
        IOperandResolver operandResolver,
        IOperandRegistry operands)
    {
        _permissionProvider = permissionProvider ?? throw new ArgumentNullException(nameof(permissionProvider));
        _policyProvider = policyProvider ?? throw new ArgumentNullException(nameof(policyProvider));
        _policyEvaluator = policyEvaluator ?? throw new ArgumentNullException(nameof(policyEvaluator));
        _operandResolver = operandResolver ?? throw new ArgumentNullException(nameof(operandResolver));
        _operands = operands ?? throw new ArgumentNullException(nameof(operands));
    }

    public async Task<IQueryable<T>> ApplyFilterAsync<T>(
        IQueryable<T> query,
        Guid userId,
        string permission,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var predicate = await BuildPredicateAsync<T>(userId, permission, cancellationToken);
        return predicate is null ? query : query.Where(predicate);
    }

    /// <summary>
    /// Predicate phạm vi cho <typeparamref name="T"/>; <c>null</c> nghĩa là không cần lọc (wildcard).
    /// </summary>
    internal async Task<Expression<Func<T, bool>>?> BuildPredicateAsync<T>(
        Guid userId,
        string permission,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var item = Expression.Parameter(typeof(T), "x");

        var permissionsByRole = await _permissionProvider.GetPermissionsByRoleAsync(userId, cancellationToken);
        var grantingRoles = permissionsByRole
            .Where(r => PermissionMatcher.Grants(r.Value, permission))
            .ToList();

        if (grantingRoles.Count == 0)
            return Expression.Lambda<Func<T, bool>>(False, item);

        if (grantingRoles.Any(r => PermissionMatcher.HasWildcard(r.Value)))
            return null;

        var scope = False;
        foreach (var (roleId, _) in grantingRoles)
        {
            List<Policy> policies;
            try
            {
                policies = (await _policyProvider.GetPoliciesByRoleAsync(roleId, cancellationToken)).ToList();
            }
            catch (UnresolvedPolicyException)
            {
                // Role cấu hình sai: đường dẫn của nó không đạt; role khác vẫn có thể cho phép.
                continue;
            }

            // Deny-by-default: role cấp permission nhưng không có policy không mở phạm vi nào.
            if (policies.Count == 0)
                continue;

            var rolePath = True;
            foreach (var policy in policies)
                rolePath = And(rolePath, await BuildPolicyAsync(item, policy, userId, cancellationToken));

            scope = Or(scope, rolePath);
        }

        return Expression.Lambda<Func<T, bool>>(scope, item);
    }

    private async Task<Expression> BuildPolicyAsync(
        ParameterExpression item,
        Policy policy,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var body = False;
        foreach (var rule in policy.Rules.OrderBy(r => r.Order))
            body = Or(body, await BuildRuleAsync(item, rule, userId, cancellationToken));
        return body;
    }

    private async Task<Expression> BuildRuleAsync(
        ParameterExpression item,
        PolicyRule rule,
        Guid userId,
        CancellationToken cancellationToken)
    {
        if (!_operands.TryGet(rule.LeftOperand, out var left) || !_operands.TryGet(rule.RightOperand, out var right))
            return False;

        var op = rule.Operator.ToUpperInvariant();
        var leftIsResource = left.Source == OperandSource.Resource;
        var rightIsResource = right.Source == OperandSource.Resource;

        if (leftIsResource && rightIsResource)
            return CompareMembers(item, left, op, right);

        if (leftIsResource)
            return CompareMember(item, left, op, await ResolveUserValueAsync(rule.RightOperand, userId, cancellationToken));

        if (rightIsResource)
        {
            // "User.X IN Resource.Y" cần collection phía resource — chưa hỗ trợ trong query.
            return op == "IN"
                ? False
                : CompareMember(item, right, Flip(op), await ResolveUserValueAsync(rule.LeftOperand, userId, cancellationToken));
        }

        // Cả hai vế thuộc user: kết quả không phụ thuộc bản ghi → hằng số, đánh giá bằng chính PolicyEvaluator.
        var single = new Policy { Code = "DataScope", Rules = [rule] };
        return await _policyEvaluator.EvaluateAsync(userId, single, new object(), cancellationToken) ? True : False;
    }

    private async Task<object?> ResolveUserValueAsync(string operand, Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            return await _operandResolver.ResolveAsync(operand, userId, cancellationToken: cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    private static Expression CompareMember(ParameterExpression item, OperandDefinition resource, string op, object? userValue)
    {
        if (userValue is null || !TryGetMember(item, resource, out var member, out var memberType))
            return False;

        var valueType = Nullable.GetUnderlyingType(memberType) ?? memberType;

        if (op == "IN")
        {
            if (userValue is string || userValue is not IEnumerable values)
                return False;

            var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(memberType))!;
            foreach (var value in values)
            {
                if (value != null && value.GetType() == valueType)
                    list.Add(value);
            }

            if (list.Count == 0)
                return False;

            var contains = Expression.Call(
                typeof(Enumerable), nameof(Enumerable.Contains), [memberType], Parameter(list, list.GetType()), member);
            return And(NotNull(member, memberType), contains);
        }

        if (userValue.GetType() != valueType)
            return False;

        var compared = Binary(op, member, Parameter(userValue, memberType));
        return compared is null ? False : And(NotNull(member, memberType), compared);
    }

    private static Expression CompareMembers(ParameterExpression item, OperandDefinition left, string op, OperandDefinition right)
    {
        if (op == "IN"
            || !TryGetMember(item, left, out var leftMember, out var leftType)
            || !TryGetMember(item, right, out var rightMember, out var rightType))
            return False;

        var leftValueType = Nullable.GetUnderlyingType(leftType) ?? leftType;
        var rightValueType = Nullable.GetUnderlyingType(rightType) ?? rightType;
        if (leftValueType != rightValueType)
            return False;

        // Đưa về cùng kiểu (nullable nếu một vế nullable) để biểu thức hợp lệ.
        var common = leftType == rightType ? leftType : typeof(Nullable<>).MakeGenericType(leftValueType);
        var compared = Binary(op, Convert(leftMember, common), Convert(rightMember, common));
        return compared is null
            ? False
            : And(And(NotNull(leftMember, leftType), NotNull(rightMember, rightType)), compared);
    }

    private static bool TryGetMember(ParameterExpression item, OperandDefinition resource, out Expression member, out Type memberType)
    {
        var property = item.Type.GetProperty(resource.Attribute, BindingFlags.Public | BindingFlags.Instance);
        if (property is null || !property.CanRead)
        {
            member = False;
            memberType = typeof(bool);
            return false;
        }

        member = Expression.Property(item, property);
        memberType = property.PropertyType;
        return true;
    }

    private static Expression? Binary(string op, Expression left, Expression right)
    {
        try
        {
            return op switch
            {
                "==" => Expression.Equal(left, right),
                "!=" => Expression.NotEqual(left, right),
                "<" => Expression.LessThan(left, right),
                "<=" => Expression.LessThanOrEqual(left, right),
                ">" => Expression.GreaterThan(left, right),
                ">=" => Expression.GreaterThanOrEqual(left, right),
                _ => null,
            };
        }
        catch (InvalidOperationException)
        {
            // Kiểu không hỗ trợ toán tử (vd. string với <) → rule không đạt.
            return null;
        }
    }

    private static string Flip(string op) => op switch
    {
        "<" => ">",
        "<=" => ">=",
        ">" => "<",
        ">=" => "<=",
        _ => op,
    };

    /// <summary>Thiếu thuộc tính không bao giờ thỏa rule (đồng bộ PolicyEvaluator; SQL NULL không lọt qua <c>!=</c>).</summary>
    private static Expression NotNull(Expression member, Type memberType) =>
        memberType.IsValueType && Nullable.GetUnderlyingType(memberType) is null
            ? True
            : Expression.NotEqual(member, Expression.Constant(null, memberType));

    private static Expression Convert(Expression expression, Type type) =>
        expression.Type == type ? expression : Expression.Convert(expression, type);

    /// <summary>Bọc giá trị trong object để EF sinh tham số SQL thay vì nhúng hằng số.</summary>
    private static Expression Parameter(object value, Type type)
    {
        var holderType = typeof(ScopeValue<>).MakeGenericType(type);
        var holder = Activator.CreateInstance(holderType, value)!;
        return Expression.Property(Expression.Constant(holder), nameof(ScopeValue<object>.Value));
    }

    private static Expression And(Expression left, Expression right)
    {
        if (IsConstant(left, false) || IsConstant(right, false)) return False;
        if (IsConstant(left, true)) return right;
        if (IsConstant(right, true)) return left;
        return Expression.AndAlso(left, right);
    }

    private static Expression Or(Expression left, Expression right)
    {
        if (IsConstant(left, true) || IsConstant(right, true)) return True;
        if (IsConstant(left, false)) return right;
        if (IsConstant(right, false)) return left;
        return Expression.OrElse(left, right);
    }

    private static bool IsConstant(Expression expression, bool value) =>
        expression is ConstantExpression { Value: bool b } && b == value;

    private sealed class ScopeValue<TValue>(TValue value)
    {
        public TValue Value { get; } = value;
    }
}
