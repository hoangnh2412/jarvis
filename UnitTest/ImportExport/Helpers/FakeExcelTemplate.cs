using Platform.DDD.Domain.Entities;

namespace UnitTest.ImportExport.Helpers;

public sealed class FakeExcelTemplate : IEntity, IExcelTemplateEntity, ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string FileName { get; set; } = "";
    public string? Location { get; set; }
    public string MappingsJson { get; set; } = "[]";
    public int HeaderRow { get; set; } = 1;
    public int StartRow { get; set; } = 2;
    public int? RowCount { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
