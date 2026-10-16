using Platform.DDD.Domain.Entities;

namespace Platform.Modules.ImportExport.EntityFramework.Entities;

/// <summary>
/// Entity Excel template mặc định đi kèm package <c>Platform.Modules.ImportExport.EntityFramework</c>.
/// Host có thể dùng trực tiếp, hoặc tự cung cấp kiểu riêng implement <see cref="IExcelTemplateEntity"/>.
/// </summary>
public class ExcelTemplate : BaseEntity<Guid>, IExcelTemplateEntity
{
    public Guid TenantId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string? Location { get; set; }

    public string MappingsJson { get; set; } = "[]";

    public int HeaderRow { get; set; } = 1;

    public int StartRow { get; set; } = 2;

    public int? RowCount { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
