using System.Globalization;

namespace Platform.ImportExport.MiniExcel.Helpers;

/// <summary>
/// Converts between Excel A1-style addresses and 1-based column indexes.
/// Column letters use base-<see cref="ExcelEngineDefaults.ExcelColumnAlphabetSize"/> (A–Z).
/// </summary>
internal static class ExcelCellAddress
{
    /// <summary>
    /// Builds a cell reference from 1-based column + row (e.g. 1,2 → <c>A2</c>).
    /// </summary>
    public static string ToCellReference(int columnIndex, int rowIndex)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(columnIndex, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(rowIndex, 1);

        var dividend = columnIndex;
        var columnName = string.Empty;
        var alphabetSize = ExcelEngineDefaults.ExcelColumnAlphabetSize;

        while (dividend > 0)
        {
            var modulo = (dividend - 1) % alphabetSize;
            columnName = (char)('A' + modulo) + columnName;
            dividend = (dividend - modulo) / alphabetSize;
        }

        return columnName + rowIndex.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Parses the column letters of a cell reference (e.g. <c>AB12</c> → 28).
    /// </summary>
    public static int? TryGetColumnIndex(string? cellReference)
    {
        if (string.IsNullOrWhiteSpace(cellReference))
            return null;

        var letters = new string(cellReference.TakeWhile(char.IsLetter).ToArray());
        if (letters.Length == 0)
            return null;

        var alphabetSize = ExcelEngineDefaults.ExcelColumnAlphabetSize;
        var index = 0;
        foreach (var ch in letters.ToUpperInvariant())
            index = index * alphabetSize + (ch - 'A' + 1);

        return index;
    }
}
