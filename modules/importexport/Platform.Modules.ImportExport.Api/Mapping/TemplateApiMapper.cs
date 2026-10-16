using Platform.Modules.ImportExport.API.Models;

namespace Platform.Modules.ImportExport.API.Mapping;

public static class TemplateApiMapper
{
    public static TemplateApiResponse ToApi(TemplateSummary template) => new()
    {
        Code = template.Code,
        Name = template.Name,
        FileName = template.FileName,
        Location = template.Location,
        HeaderRow = template.HeaderRow,
        StartRow = template.StartRow,
        RowCount = template.RowCount,
        SheetName = template.SheetName,
        Mappings = template.Mappings,
        Sheets = template.Sheets,
        CreatedAt = template.CreatedAt,
        UpdatedAt = template.UpdatedAt
    };
}
