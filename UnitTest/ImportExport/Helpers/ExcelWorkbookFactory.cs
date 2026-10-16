using MiniExcelLibs;

namespace UnitTest.ImportExport.Helpers;

/// <summary>In-memory .xlsx create/read for import/export tests.</summary>
public static class ExcelWorkbookFactory
{
    public static MemoryStream Create(
        IEnumerable<Dictionary<string, object?>> rows,
        string sheetName = "Customers")
    {
        var ms = new MemoryStream();
        MiniExcel.SaveAs(ms, rows, sheetName: sheetName);
        ms.Position = 0;
        return ms;
    }

    public static IReadOnlyList<string> GetSheetNames(byte[] content)
    {
        using var ms = new MemoryStream(content);
        return MiniExcelLibs.MiniExcel.GetSheetNames(ms);
    }

    public static IReadOnlyList<IDictionary<string, object?>> ReadSheet(
        byte[] content,
        string? sheetName = null)
    {
        using var ms = new MemoryStream(content);
        var sheets = MiniExcelLibs.MiniExcel.GetSheetNames(ms);
        ms.Position = 0;
        var name = sheetName ?? sheets[0];
        return MiniExcel.Query(ms, useHeaderRow: true, sheetName: name)
            .Cast<IDictionary<string, object>>()
            .Select(r => (IDictionary<string, object?>)r.ToDictionary(
                kv => kv.Key,
                kv => (object?)kv.Value,
                StringComparer.OrdinalIgnoreCase))
            .ToList();
    }
}
