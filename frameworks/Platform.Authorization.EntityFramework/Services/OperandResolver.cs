namespace Platform.Authorization.EntityFramework.Services;

using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Platform.Authorization.Abstractions;
using Platform.Authorization.EntityFramework.Persistence;
using Platform.Authorization.Operands;

/// <summary>
/// Giải quyết các operand được đăng ký (<see cref="IOperandRegistry"/>) chống lại kho lưu trữ Danh tính được chia sẻ
/// và đối tượng tài nguyên. Các operand chưa được đăng ký bị từ chối — danh sách trắng được thực thi tại thời gian chạy,
/// không chỉ khi quy tắc được lưu (ADR D10).
/// </summary>
public sealed class OperandResolver : IOperandResolver
{
    private readonly IdentityDbContextBase _context;
    private readonly IOperandRegistry _registry;

    public OperandResolver(IdentityDbContextBase context, IOperandRegistry registry)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public async Task<object?> ResolveAsync(
        string operand,
        Guid userId,
        object? resource = null,
        CancellationToken cancellationToken = default)
    {
        if (!_registry.TryGet(operand, out var definition))
        {
            throw new InvalidOperationException($"Operand '{operand}' không được đăng ký.");
        }

        return definition.Source switch
        {
            OperandSource.UserSubject => userId,
            OperandSource.UserColumn => await ResolveUserColumnAsync(definition, userId, cancellationToken),
            OperandSource.UserClaim => await ResolveUserClaimAsync(definition, userId, cancellationToken),
            OperandSource.Resource => ResolveResource(definition, resource),
            _ => throw new InvalidOperationException($"Nguồn operand không được hỗ trợ {definition.Source} cho '{operand}'."),
        };
    }

    private async Task<object?> ResolveUserColumnAsync(OperandDefinition definition, Guid userId, CancellationToken cancellationToken)
    {
        // ADR Q3: TenantId / OrgId là các cột AspNetUsers.
        var user = _context.Users.Where(u => u.Id == userId);
        return definition.Attribute switch
        {
            "TenantId" => await user.Select(u => u.TenantId).FirstOrDefaultAsync(cancellationToken),
            "OrgId" => await user.Select(u => u.OrgId).FirstOrDefaultAsync(cancellationToken),
            _ => throw new InvalidOperationException($"Cột người dùng '{definition.Attribute}' không được ánh xạ."),
        };
    }

    private async Task<object?> ResolveUserClaimAsync(OperandDefinition definition, Guid userId, CancellationToken cancellationToken)
    {
        var claimType = definition.ClaimType ?? definition.Attribute;
        var value = await _context.UserClaims
            .Where(c => c.UserId == userId && c.ClaimType == claimType)
            .Select(c => c.ClaimValue)
            .FirstOrDefaultAsync(cancellationToken);

        if (value is null)
        {
            return null;
        }

        if (definition.Kind == OperandValueKind.Collection)
        {
            return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(ParseScalar)
                .ToList();
        }

        return ParseScalar(value);
    }

    private static object? ResolveResource(OperandDefinition definition, object? resource)
    {
        if (resource == null)
        {
            throw new InvalidOperationException($"Cannot resolve '{definition.Path}' without a resource object.");
        }

        return resource.GetType().GetProperty(definition.Attribute)?.GetValue(resource);
    }

    private static object ParseScalar(string value)
    {
        if (Guid.TryParse(value, out var guid))
        {
            return guid;
        }

        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var number))
        {
            return number;
        }

        return value;
    }
}
