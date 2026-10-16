using System.Globalization;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Platform.ImportExport.MiniExcel.Helpers;

/// <summary>
/// Low-level OpenXml worksheet mutations used when preparing export templates
/// (create cells, write text, drop sample rows). Keeps address math in <see cref="ExcelCellAddress"/>.
/// </summary>
internal static class OpenXmlSheetEditor
{
    /// <summary>Maps 1-based column index â†’ header text for the given header row.</summary>
    public static Dictionary<int, string> ReadHeaderMap(
        SheetData sheetData,
        int headerRow,
        SharedStringTable? sharedStrings)
    {
        var map = new Dictionary<int, string>();
        var row = sheetData.Elements<Row>()
            .FirstOrDefault(r => r.RowIndex?.Value == (uint)headerRow);
        if (row is null)
            return map;

        foreach (var cell in row.Elements<Cell>())
        {
            var col = ExcelCellAddress.TryGetColumnIndex(cell.CellReference?.Value);
            if (col is null)
                continue;

            var text = GetCellText(cell, sharedStrings);
            if (!string.IsNullOrWhiteSpace(text))
                map[col.Value] = text.Trim();
        }

        return map;
    }

    /// <summary>Finds column index by <see cref="ColumnMapping.ColumnName"/> or <see cref="ColumnMapping.FieldName"/>.</summary>
    public static bool TryFindColumnIndex(
        IReadOnlyDictionary<int, string> headerMap,
        ColumnMapping mapping,
        out int columnIndex)
    {
        foreach (var (index, header) in headerMap)
        {
            if (header.Equals(mapping.ColumnName, StringComparison.OrdinalIgnoreCase)
                || header.Equals(mapping.FieldName, StringComparison.OrdinalIgnoreCase))
            {
                columnIndex = index;
                return true;
            }
        }

        columnIndex = 0;
        return false;
    }

    /// <summary>Gets an existing cell or creates one (keeps row/column order for OpenXml).</summary>
    public static Cell GetOrCreateCell(SheetData sheetData, int rowIndex, int columnIndex)
    {
        var row = sheetData.Elements<Row>()
            .FirstOrDefault(r => r.RowIndex?.Value == (uint)rowIndex);
        if (row is null)
        {
            row = new Row { RowIndex = (uint)rowIndex };
            InsertRow(sheetData, row);
        }

        var reference = ExcelCellAddress.ToCellReference(columnIndex, rowIndex);
        var cell = row.Elements<Cell>()
            .FirstOrDefault(c => string.Equals(c.CellReference?.Value, reference, StringComparison.OrdinalIgnoreCase));
        if (cell is not null)
            return cell;

        cell = new Cell { CellReference = reference };
        InsertCell(row, cell);
        return cell;
    }

    /// <summary>Writes inline string into the cell (preserves existing <c>StyleIndex</c> when set).</summary>
    public static void SetCellText(Cell cell, string text)
    {
        cell.DataType = CellValues.InlineString;
        cell.CellValue = null;
        cell.InlineString = new InlineString(new Text { Text = text });
    }

    /// <summary>Removes rows below <paramref name="startRow"/> (sample data under the placeholder row).</summary>
    public static void RemoveRowsAfter(SheetData sheetData, int startRow)
    {
        var toRemove = sheetData.Elements<Row>()
            .Where(r => r.RowIndex?.Value > (uint)startRow)
            .ToList();
        foreach (var row in toRemove)
            row.Remove();
    }

    private static void InsertRow(SheetData sheetData, Row row)
    {
        var next = sheetData.Elements<Row>().FirstOrDefault(r => r.RowIndex?.Value > row.RowIndex?.Value);
        if (next is null)
            sheetData.Append(row);
        else
            sheetData.InsertBefore(row, next);
    }

    private static void InsertCell(Row row, Cell cell)
    {
        var cellCol = ExcelCellAddress.TryGetColumnIndex(cell.CellReference?.Value) ?? int.MaxValue;
        var next = row.Elements<Cell>().FirstOrDefault(c =>
            (ExcelCellAddress.TryGetColumnIndex(c.CellReference?.Value) ?? int.MaxValue) > cellCol);
        if (next is null)
            row.Append(cell);
        else
            row.InsertBefore(cell, next);
    }

    private static string? GetCellText(Cell cell, SharedStringTable? sharedStrings)
    {
        if (cell.DataType?.Value == CellValues.InlineString)
            return cell.InlineString?.Text?.Text ?? cell.InlineString?.InnerText;

        var raw = cell.CellValue?.InnerText;
        if (string.IsNullOrEmpty(raw))
            return null;

        if (cell.DataType?.Value == CellValues.SharedString
            && sharedStrings is not null
            && int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index))
        {
            return sharedStrings.ElementAt(index).InnerText;
        }

        return raw;
    }
}
