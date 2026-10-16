using Platform.Modules.ImportExport.Domain.Constants;
using Platform.Modules.ImportExport.Domain.Exceptions;
using Platform.Modules.ImportExport.Domain.Models;

namespace Platform.Modules.ImportExport.Application.Templates;

/// <summary>File extension helpers and sheetName rules for template configuration.</summary>
public static class TemplateFileTypeHelper
{
    public static bool IsCsv(string? fileName) =>
        string.Equals(Path.GetExtension(fileName), EngineMessages.CsvExtension, StringComparison.OrdinalIgnoreCase);

    public static bool IsExcel(string? fileName) =>
        string.Equals(Path.GetExtension(fileName), EngineMessages.XlsxExtension, StringComparison.OrdinalIgnoreCase);

    public static void ValidateSheetNameForTemplate(
        string? templateFileName,
        string? sheetName,
        IReadOnlyList<SheetTemplate> sheets)
    {
        if (string.IsNullOrWhiteSpace(templateFileName) || IsCsv(templateFileName))
            return;

        if (!IsExcel(templateFileName))
            return;

        if (sheets.Count > 0)
            return;

        if (string.IsNullOrWhiteSpace(sheetName))
            throw new InvalidTemplateException(EngineMessages.SheetNameRequiredForExcel);
    }
}
