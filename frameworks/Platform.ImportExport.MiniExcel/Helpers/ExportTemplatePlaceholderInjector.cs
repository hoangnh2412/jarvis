using System.IO.Compression;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Platform.ImportExport.MiniExcel.Helpers;

/// <summary>
/// Chuẩn bị file .xlsx mẫu cho MiniExcel <c>SaveAsByTemplate</c> (giữ style / sheet name).
/// </summary>
/// <remarks>
/// <list type="number">
/// <item>Scan zip XML — nếu đã có <c>{{rows.FieldName}}</c> đủ mapping → trả nguyên bytes.</item>
/// <item>Thiếu token bắt buộc khi đã có marker <c>{{rows.</c> → <see cref="InvalidTemplateException"/>.</item>
/// <item>Chưa có placeholder → inject qua OpenXML editor theo HeaderRow/StartRow.</item>
/// </list>
/// Gọi từ <c>MiniExcelExportProcessor</c> khi <c>TemplateContent</c> khác rỗng.
/// </remarks>
internal static class ExportTemplatePlaceholderInjector
{
    private static readonly string RowsMarker =
        "{{" + ExportTemplateConstants.RowsCollectionName + ".";

    /// <summary>
    /// Trả bytes sẵn sàng cho MiniExcel: template gốc nếu placeholder hợp lệ, hoặc bản đã inject.
    /// Nên gọi một lần / export.
    /// </summary>
    public static byte[] EnsurePlaceholders(
        byte[] template,
        IReadOnlyList<ColumnMapping> mappings,
        int headerRow,
        int startRow,
        string? preferredSheetName)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(mappings);

        if (template.Length == 0)
            throw new InvalidTemplateException(EngineMessages.TemplateContentRequired);

        if (TryValidateExistingPlaceholders(template, mappings))
            return template;

        return InjectPlaceholders(template, mappings, headerRow, startRow, preferredSheetName);
    }

    /// <summary>
    /// Single zip pass: true when <c>{{rows.</c> exists and every mapping field token is present.
    /// </summary>
    private static bool TryValidateExistingPlaceholders(
        byte[] template,
        IReadOnlyList<ColumnMapping> mappings)
    {
        var xml = ReadAllXmlEntries(template);
        if (xml is null || !xml.Contains(RowsMarker, StringComparison.Ordinal))
            return false;

        foreach (var mapping in mappings)
        {
            var token = Placeholder(mapping.FieldName);
            if (!xml.Contains(token, StringComparison.Ordinal))
                throw new InvalidTemplateException(
                    EngineMessages.ExportTemplatePlaceholderMissing(mapping.FieldName));
        }

        return true;
    }

    /// <summary>Reads all <c>.xml</c> parts from the xlsx zip into one string for token search.</summary>
    private static string? ReadAllXmlEntries(byte[] template)
    {
        using var zip = new ZipArchive(new MemoryStream(template), ZipArchiveMode.Read);
        StringBuilder? sb = null;
        foreach (var entry in zip.Entries)
        {
            if (!entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                continue;

            sb ??= new StringBuilder(capacity: (int)Math.Min(
                entry.Length * 2,
                ExcelEngineDefaults.ZipXmlScanBufferCapBytes));
            using var reader = new StreamReader(entry.Open(), Encoding.UTF8);
            sb.Append(reader.ReadToEnd());
        }

        return sb?.ToString();
    }

    /// <summary>
    /// Clones the template, writes placeholders on <paramref name="startRow"/>, drops rows below.
    /// </summary>
    private static byte[] InjectPlaceholders(
        byte[] template,
        IReadOnlyList<ColumnMapping> mappings,
        int headerRow,
        int startRow,
        string? preferredSheetName)
    {
        var buffer = new MemoryStream(template.Length + ExcelEngineDefaults.TemplateEditStreamPaddingBytes);
        buffer.Write(template, 0, template.Length);
        buffer.Position = 0;

        using (var document = SpreadsheetDocument.Open(buffer, isEditable: true))
        {
            var workbookPart = document.WorkbookPart
                ?? throw new InvalidTemplateException(EngineMessages.FailedToExportExcel);

            var sheet = ResolveSheet(workbookPart, preferredSheetName);
            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id!);
            var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>()
                ?? throw new InvalidTemplateException(EngineMessages.FailedToExportExcel);

            var sharedStrings = workbookPart.SharedStringTablePart?.SharedStringTable;
            var headerMap = OpenXmlSheetEditor.ReadHeaderMap(sheetData, headerRow, sharedStrings);

            foreach (var mapping in mappings)
            {
                if (!OpenXmlSheetEditor.TryFindColumnIndex(headerMap, mapping, out var columnIndex))
                {
                    throw new InvalidTemplateException(
                        EngineMessages.ColumnNotFound(mapping.ColumnName));
                }

                var cell = OpenXmlSheetEditor.GetOrCreateCell(sheetData, startRow, columnIndex);
                OpenXmlSheetEditor.SetCellText(cell, Placeholder(mapping.FieldName));
            }

            OpenXmlSheetEditor.RemoveRowsAfter(sheetData, startRow);
            worksheetPart.Worksheet.Save();
        }

        return buffer.ToArray();
    }

    /// <summary>Builds MiniExcel collection placeholder, e.g. <c>{{rows.CustomerCode}}</c>.</summary>
    private static string Placeholder(string fieldName) =>
        "{{" + ExportTemplateConstants.RowsCollectionName + "." + fieldName + "}}";

    /// <summary>Picks worksheet by preferred name, or the first sheet when name is null.</summary>
    private static Sheet ResolveSheet(WorkbookPart workbookPart, string? preferredSheetName)
    {
        var sheets = workbookPart.Workbook.Sheets?.Elements<Sheet>().ToList()
            ?? throw new InvalidTemplateException(EngineMessages.FailedToExportExcel);

        if (sheets.Count == 0)
            throw new InvalidTemplateException(EngineMessages.SheetNotFound("(any)", []));

        if (!string.IsNullOrWhiteSpace(preferredSheetName))
        {
            var match = sheets.FirstOrDefault(s =>
                string.Equals(s.Name?.Value, preferredSheetName.Trim(), StringComparison.OrdinalIgnoreCase));
            if (match is not null)
                return match;

            throw new InvalidTemplateException(
                EngineMessages.SheetNotFound(preferredSheetName, sheets.Select(s => s.Name?.Value ?? "")));
        }

        return sheets[0];
    }
}
