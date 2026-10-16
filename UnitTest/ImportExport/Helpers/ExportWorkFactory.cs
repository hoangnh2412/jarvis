namespace UnitTest.ImportExport.Helpers;

public static class ExportWorkFactory
{
    public static ExportWork<Dictionary<string, object?>> Create(
        IEnumerable<Dictionary<string, object?>> data,
        IReadOnlyCollection<ColumnMapping> mappings,
        string fileName,
        string? sheetName,
        int? rowCount = null) =>
        new()
        {
            Data = data,
            Mappings = mappings,
            FileName = fileName,
            SheetName = sheetName,
            RowCount = rowCount
        };
}
