namespace UnitTest.ImportExport.Validation;

/// <summary>
/// Unit test cho factory <see cref="ImportResult.From"/> — gom sheet result + errors thành response cuối.
/// </summary>
public class ImportResultTests
{
    /// <summary>
    /// Khi có lỗi, vẫn giữ các dòng valid đã đọc được.
    /// Setup: 1 sheet (1 valid / 1 failed) + 1 ImportRowError.
    /// Kỳ vọng: Sheets/Rows/Errors đúng count; ErrorSummary trên sheet chứa ErrorCode tương ứng.
    /// </summary>
    [Fact]
    public void From_KeepsValidRows_WhenErrorsPresent()
    {
        var sheets = new[]
        {
            new SheetImportResult
            {
                SheetName = "Sheet1",
                Rows = [new Dictionary<string, object?> { ["A"] = 1 }],
                ProcessedRowCount = 2,
                FailedRowCount = 1,
                ValidRowCount = 1
            }
        };
        var errors = new[]
        {
            new ImportRowError(3, "x", ErrorCode: ImportErrorCodes.FieldRequired, SheetName: "Sheet1")
        };

        var result = ImportResult.From(sheets, errors);

        Assert.Single(result.Sheets);
        Assert.Single(result.Sheets[0].Rows);
        Assert.Single(result.Errors);
        Assert.Equal(1, result.FailedRowCount);
        Assert.Equal(1, result.ValidRowCount);
        Assert.Single(result.Sheets[0].ErrorSummary);
        Assert.Equal(ImportErrorCodes.FieldRequired, result.Sheets[0].ErrorSummary[0].ErrorCode);
    }

    /// <summary>
    /// ErrorSummary được nhóm theo từng sheet và gộp theo ErrorCode.
    /// Setup: 2 sheet; Customers có 2 lỗi FieldRequired (row 2,3); Orders có 1 InvalidFormat.
    /// Kỳ vọng: Customers.ErrorSummary.Count = 2 + RowNumbers = [2,3]; Orders có InvalidFormat; tổng Failed = 3.
    /// </summary>
    [Fact]
    public void From_NestsErrorSummary_PerSheet()
    {
        var sheets = new[]
        {
            new SheetImportResult { SheetName = "Customers", ProcessedRowCount = 2, FailedRowCount = 2 },
            new SheetImportResult { SheetName = "Orders", ProcessedRowCount = 1, FailedRowCount = 1 }
        };
        var errors = new[]
        {
            new ImportRowError(2, "Phone required", ColumnName: "Phone", FieldName: "Phone",
                ErrorCode: ImportErrorCodes.FieldRequired, SheetName: "Customers"),
            new ImportRowError(3, "Phone required", ColumnName: "Phone", FieldName: "Phone",
                ErrorCode: ImportErrorCodes.FieldRequired, SheetName: "Customers"),
            new ImportRowError(2, "Qty bad", ColumnName: "Qty", FieldName: "Qty",
                ErrorCode: ImportErrorCodes.InvalidFormat, SheetName: "Orders")
        };

        var result = ImportResult.From(sheets, errors);

        Assert.Single(result.Sheets[0].ErrorSummary);
        Assert.Equal(2, result.Sheets[0].ErrorSummary[0].Count);
        Assert.Equal([2, 3], result.Sheets[0].ErrorSummary[0].RowNumbers);
        Assert.Single(result.Sheets[1].ErrorSummary);
        Assert.Equal(ImportErrorCodes.InvalidFormat, result.Sheets[1].ErrorSummary[0].ErrorCode);
        Assert.Equal(3, result.FailedRowCount);
    }
}
