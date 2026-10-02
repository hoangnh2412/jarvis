namespace Platform.Authorization.Operands;

/// <summary>
/// Thu thập các định nghĩa operand tại thời gian khởi động. Platform chỉ đóng góp các operand chung
/// (<see cref="AddPlatformDefaults"/>); các sản phẩm và mô-đun miền thêm các operand kinh doanh riêng của họ
/// thông qua <c>AddPlatformAuthorizationOperands(...)</c>.
/// </summary>
public sealed class OperandCatalog
{
    private static readonly HashSet<string> UserColumns = new(StringComparer.Ordinal) { "TenantId", "OrgId" };

    private readonly Dictionary<string, OperandDefinition> _items = new(StringComparer.Ordinal);

    internal IReadOnlyCollection<OperandDefinition> Items => _items.Values;

    /// <summary>Thuộc tính người dùng được đọc từ AspNetUserClaims (loại claim mặc định là tên thuộc tính).</summary>
    public OperandCatalog AddUserClaim(string attribute, OperandValueKind kind = OperandValueKind.Scalar, string? claimType = null) =>
        Add(new OperandDefinition(OperandDefinition.UserPrefix + attribute, OperandSource.UserClaim, kind, claimType ?? attribute));

    /// <summary>Thuộc tính công khai của đối tượng tài nguyên được chuyển cho engine.</summary>
    public OperandCatalog AddResource(string attribute, OperandValueKind kind = OperandValueKind.Scalar) =>
        Add(new OperandDefinition(OperandDefinition.ResourcePrefix + attribute, OperandSource.Resource, kind));

    internal OperandCatalog AddPlatformDefaults()
    {
        Add(new OperandDefinition("User.Id", OperandSource.UserSubject));
        foreach (var column in UserColumns)
            Add(new OperandDefinition(OperandDefinition.UserPrefix + column, OperandSource.UserColumn));

        AddResource("TenantId");
        AddResource("OrgId");
        AddResource("CreatedBy");
        return this;
    }

    private OperandCatalog Add(OperandDefinition definition)
    {
        ValidateAttribute(definition.Attribute, definition.Path);

        if (_items.TryGetValue(definition.Path, out var existing))
        {
            if (existing != definition)
                throw new InvalidOperationException(
                    $"Operand '{definition.Path}' đã được đăng ký với định nghĩa khác ({existing}).");
            return this;
        }

        _items.Add(definition.Path, definition);
        return this;
    }

    private static void ValidateAttribute(string attribute, string path)
    {
        if (string.IsNullOrWhiteSpace(attribute) || attribute.Contains('.') || attribute.Any(char.IsWhiteSpace))
            throw new ArgumentException($"Operand không hợp lệ '{path}': thuộc tính phải là một định danh duy nhất không trống.", nameof(attribute));
    }
}
