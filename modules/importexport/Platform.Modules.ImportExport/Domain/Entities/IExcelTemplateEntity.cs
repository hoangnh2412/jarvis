namespace Platform.Modules.ImportExport.Domain.Entities;

/// <summary>
/// Host-owned Excel template row. Consumer maps this to <c>excel_templates</c> (or renamed table).
/// </summary>
public interface IExcelTemplateEntity
{
    Guid Id { get; set; }

    Guid TenantId { get; set; }

    string Code { get; set; }

    string Name { get; set; }

    string FileName { get; set; }

    string? Location { get; set; }

    string MappingsJson { get; set; }

    int HeaderRow { get; set; }

    int StartRow { get; set; }

    int? RowCount { get; set; }

    bool IsActive { get; set; }

    DateTimeOffset CreatedAt { get; set; }

    DateTimeOffset UpdatedAt { get; set; }
}