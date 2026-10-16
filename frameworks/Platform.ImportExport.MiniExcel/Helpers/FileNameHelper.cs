using MiniExcelLibs;

namespace Platform.ImportExport.MiniExcel.Helpers;

internal static class FileNameHelper
{
    /// <summary>
    /// MiniExcel cannot infer CSV from stream bytes — callers must pass <see cref="ExcelType.CSV"/>.
    /// </summary>
    public static ExcelType ResolveExcelType(string? fileName) =>
        !string.IsNullOrWhiteSpace(fileName) &&
        fileName.EndsWith(EngineMessages.CsvExtension, StringComparison.OrdinalIgnoreCase)
            ? ExcelType.CSV
            : ExcelType.XLSX;

    public static string EnsureXlsxExtension(string fileName)
    {
        return fileName.EndsWith(EngineMessages.XlsxExtension, StringComparison.OrdinalIgnoreCase)
            ? fileName
            : $"{Path.GetFileNameWithoutExtension(fileName)}{EngineMessages.XlsxExtension}";
    }

    public static bool IsSupportedImportFile(string fileName, IEnumerable<string> supportedExtensions)
    {
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension))
            return false;

        return supportedExtensions.Any(ext =>
            extension.Equals(NormalizeExtension(ext), StringComparison.OrdinalIgnoreCase));
    }

    private static string NormalizeExtension(string extension) =>
        extension.StartsWith('.') ? extension : "." + extension;
}
