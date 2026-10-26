namespace Platform.Tenants.Models;

/// <summary>Cột chuẩn trả về cho API vận hành. Danh sách không kèm chuỗi kết nối.</summary>
public sealed class TenantDescriptor
{
    public Guid Id { get; init; }

    public string Code { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public Guid? ParentTenantId { get; init; }

    /// <summary>Chỉ có trên đọc một bản ghi và sau khi ghi. Danh sách để null.</summary>
    public string? ConnectionString { get; init; }
}
