namespace Platform.Modules.ImportExport.EntityFramework.Configuration;

/// <summary>Tuỳ chọn tên bảng / schema cho entity Excel template mặc định.</summary>
public sealed class ExcelTemplateModelBuilderConfigurationOptions
{
    public string TableName { get; set; } = "excel_templates";

    public string? Schema { get; set; }
}
