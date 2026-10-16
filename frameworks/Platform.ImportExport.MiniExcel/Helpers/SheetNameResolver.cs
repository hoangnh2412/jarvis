using MiniExcelLibs;

namespace Platform.ImportExport.MiniExcel.Helpers;

/// <summary>
/// Đọc tên sheet workbook qua MiniExcel và rewind stream để <c>QueryAsync</c> sau đó còn đọc được.
/// </summary>
/// <remarks>
/// CSV không có sheet thật → trả tên synthetic <see cref="EngineMessages.DefaultExportSheetName"/>.
/// </remarks>
internal static class SheetNameResolver
{
    /// <summary>
    /// Lists sheet names in the workbook (CSV → synthetic single sheet name).
    /// Resets stream position when seekable.
    /// </summary>
    public static IReadOnlyList<string> GetSheetNames(Stream stream, string? fileName = null)
    {
        if (!string.IsNullOrWhiteSpace(fileName) &&
            fileName.EndsWith(EngineMessages.CsvExtension, StringComparison.OrdinalIgnoreCase))
        {
            return [EngineMessages.DefaultExportSheetName];
        }

        var names = MiniExcelLibs.MiniExcel.GetSheetNames(stream);
        ResetStream(stream);
        return names;
    }

    /// <summary>
    /// Resolves an optional sheet name (case-insensitive) by reading sheet names once.
    /// Null/empty request → null (caller decides first-sheet vs all-sheets).
    /// </summary>
    public static string? Resolve(Stream stream, string? requestedSheetName, string? fileName = null)
    {
        if (string.IsNullOrWhiteSpace(requestedSheetName))
            return null;

        if (!string.IsNullOrWhiteSpace(fileName) &&
            fileName.EndsWith(EngineMessages.CsvExtension, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var sheetNames = GetSheetNames(stream, fileName);
        return Match(requestedSheetName, sheetNames)
               ?? throw new InvalidTemplateException(
                   EngineMessages.SheetNotFound(requestedSheetName, sheetNames));
    }

    /// <summary>Case-insensitive match against an already-loaded sheet name list (no I/O).</summary>
    public static string? Match(string requestedSheetName, IReadOnlyList<string> sheetNames)
    {
        var requested = requestedSheetName.Trim();
        for (var i = 0; i < sheetNames.Count; i++)
        {
            var name = sheetNames[i];
            if (string.Equals(name, requested, StringComparison.OrdinalIgnoreCase))
                return name;
        }

        return null;
    }

    /// <summary>Rewinds a seekable stream to the beginning (required between MiniExcel calls).</summary>
    public static void ResetStream(Stream stream)
    {
        if (stream.CanSeek)
            stream.Position = 0;
    }
}
