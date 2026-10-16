using Platform.Modules.ImportExport.API.Models;

namespace Platform.Modules.ImportExport.API.Constants;

/// <summary>Multipart / form field names — keep in sync with form DTOs via nameof.</summary>
public static class ImportExportFormFieldNames
{
    public const string File = nameof(ImportApiRequest.File);
    public const string TemplateCode = nameof(ImportApiRequest.TemplateCode);
    public const string Mappings = nameof(CreateTemplateForm.Mappings);
}

public static class ImportExportApiMessages
{
    public const string FileRequired = "File is required.";

    public const string TemplateCodeRequired = "TemplateCode is required.";

    public const string TemplateNameRequired = "Template name is required.";

    public static string ImportFailedWithRowErrors(int failedRowCount) =>
        $"Import failed: {failedRowCount} row(s) have validation errors.";

    public const string ImportCompleted = "Import completed.";
}
