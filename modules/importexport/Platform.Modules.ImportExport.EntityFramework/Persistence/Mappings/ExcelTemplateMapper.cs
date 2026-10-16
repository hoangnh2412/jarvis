using Platform.Modules.ImportExport.EntityFramework.Persistence.Models;

namespace Platform.Modules.ImportExport.EntityFramework.Persistence.Mappings;

public static class ExcelTemplateMapper
{
    public static string SerializeLayout(
        IReadOnlyList<ColumnMapping> mappings,
        IReadOnlyList<SheetTemplate>? sheets = null,
        string? sheetName = null) =>
        TemplateLayoutParser.SerializeLayout(mappings, sheets, sheetName);

    public static (IReadOnlyList<ColumnMapping> Mappings, IReadOnlyList<SheetTemplate> Sheets, string? SheetName)
        DeserializeLayout(string json) =>
        TemplateLayoutParser.DeserializeLayout(json);

    internal static TemplateSummary ToSummary(TemplateSummaryRow row)
    {
        var (mappings, sheets, sheetName) = DeserializeLayout(row.MappingsJson);
        return new TemplateSummary
        {
            TenantId = row.TenantId,
            Code = row.Code,
            Name = row.Name,
            FileName = row.FileName,
            Location = row.Location,
            HeaderRow = row.HeaderRow,
            StartRow = row.StartRow,
            RowCount = row.RowCount,
            SheetName = sheetName,
            CreatedAt = row.CreatedAt,
            UpdatedAt = row.UpdatedAt,
            Mappings = mappings,
            Sheets = sheets
        };
    }

    public static TemplateDefinition ToDefinition(IExcelTemplateEntity entity, byte[]? content = null)
    {
        var (mappings, sheets, sheetName) = DeserializeLayout(entity.MappingsJson);
        return new TemplateDefinition
        {
            TenantId = entity.TenantId,
            Code = entity.Code,
            Name = entity.Name,
            FileName = entity.FileName,
            Location = entity.Location,
            Content = content,
            Mappings = mappings,
            Sheets = sheets,
            HeaderRow = entity.HeaderRow,
            StartRow = entity.StartRow,
            RowCount = entity.RowCount,
            SheetName = sheetName,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public static TemplateSummary ToSummary(IExcelTemplateEntity entity) =>
        ToSummary(new TemplateSummaryRow
        {
            TenantId = entity.TenantId,
            Code = entity.Code,
            Name = entity.Name,
            FileName = entity.FileName,
            Location = entity.Location,
            MappingsJson = entity.MappingsJson,
            HeaderRow = entity.HeaderRow,
            StartRow = entity.StartRow,
            RowCount = entity.RowCount,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        });

    public static TemplateDetail ToDetail(IExcelTemplateEntity entity, byte[] content)
    {
        var summary = ToSummary(entity);
        return new TemplateDetail
        {
            TenantId = summary.TenantId,
            Code = summary.Code,
            Name = summary.Name,
            FileName = summary.FileName,
            Location = summary.Location,
            HeaderRow = summary.HeaderRow,
            StartRow = summary.StartRow,
            RowCount = summary.RowCount,
            SheetName = summary.SheetName,
            Mappings = summary.Mappings,
            Sheets = summary.Sheets,
            CreatedAt = summary.CreatedAt,
            UpdatedAt = summary.UpdatedAt,
            Content = content
        };
    }
}
