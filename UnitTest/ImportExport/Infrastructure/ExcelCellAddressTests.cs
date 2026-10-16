using Platform.ImportExport.MiniExcel.Helpers;

namespace UnitTest.ImportExport.Infrastructure;

/// <summary>
/// Unit test cho helper địa chỉ ô Excel (<see cref="ExcelCellAddress"/>).
/// </summary>
public class ExcelCellAddressTests
{
    /// <summary>
    /// Chuyển (columnIndex, rowIndex) → chuỗi tham chiếu ô (A1, Z2, AA1, AB10).
    /// </summary>
    [Theory]
    [InlineData(1, 1, "A1")]
    [InlineData(26, 2, "Z2")]
    [InlineData(27, 1, "AA1")]
    [InlineData(28, 10, "AB10")]
    public void ToCellReference_MapsColumnAndRow(int column, int row, string expected) =>
        Assert.Equal(expected, ExcelCellAddress.ToCellReference(column, row));

    /// <summary>
    /// Parse phần chữ cái của cell ref → column index (1-based).
    /// Input null/rỗng/"12" (không có chữ) → null.
    /// </summary>
    [Theory]
    [InlineData("A1", 1)]
    [InlineData("Z2", 26)]
    [InlineData("AA1", 27)]
    [InlineData("AB10", 28)]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("12", null)]
    public void TryGetColumnIndex_ParsesLetters(string? cellRef, int? expected) =>
        Assert.Equal(expected, ExcelCellAddress.TryGetColumnIndex(cellRef));
}
