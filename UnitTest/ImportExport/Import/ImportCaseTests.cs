namespace UnitTest.ImportExport.Import;

/// <summary>
/// Các case import end-to-end (từ legacy note).
/// Payload dùng <see cref="ImportWork"/> hiện tại; file mẫu là workbook Customers chuẩn.
/// </summary>
public class ImportCaseTests
{
    /// <summary>
    /// Case 1 — Happy path: import đủ mọi dòng hợp lệ.
    /// Setup: workbook 3 customer hợp lệ + template mặc định (đủ mapping).
    /// Kỳ vọng: ValidRowCount = 3, FailedRowCount = 0, sheet trả về đúng 3 row.
    /// </summary>
    [Fact]
    public async Task Case1_Import_HappyPath_ImportsAllRows()
    {
        await using var file = CustomerTestData.SampleCustomersWorkbook();
        var result = await ProcessorTestFactory.CreateImport().ImportAsync(
            ImportWorkFactory.Create(file, ImportWorkFactory.Template()),
            CancellationToken.None);

        Assert.Equal(3, result.ValidRowCount);
        Assert.Equal(0, result.FailedRowCount);
        Assert.Equal(3, result.Sheets[0].Rows.Count);
    }

    [Fact]
    public async Task Import_CsvFile_ImportsAllRows()
    {
        await using var file = CustomerTestData.SampleCustomersCsv();
        var result = await ProcessorTestFactory.CreateImport().ImportAsync(
            ImportWorkFactory.Create(
                file,
                ImportWorkFactory.Template(sheetName: string.Empty),
                fileName: "customers.csv"),
            CancellationToken.None);

        Assert.Equal(3, result.ValidRowCount);
        Assert.Equal(0, result.FailedRowCount);
        Assert.Equal(EngineMessages.DefaultExportSheetName, result.Sheets[0].SheetName);
    }

    /// <summary>
    /// CSV header có khoảng trắng đầu/cuối vẫn khớp ColumnMapping.
    /// </summary>
    [Fact]
    public async Task Import_CsvFile_PaddedHeaders_ImportsAllRows()
    {
        var content =
            """
             CustomerCode , CustomerName ,Email,Phone,BirthDate
            CUS001,Nguyen Van A,a@example.com,0901234567,1990-01-01
            CUS002,Tran Thi B,b@example.com,0912345678,1992-05-10
            """;
        await using var file = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        var result = await ProcessorTestFactory.CreateImport().ImportAsync(
            ImportWorkFactory.Create(
                file,
                ImportWorkFactory.Template(sheetName: string.Empty),
                fileName: "customers-padded.csv"),
            CancellationToken.None);

        Assert.Equal(2, result.ValidRowCount);
        Assert.Equal(0, result.FailedRowCount);
    }

    /// <summary>
    /// Case 2 — Giới hạn số dòng bằng <c>RowCount</c>.
    /// Setup: workbook 3 dòng nhưng template <c>rowCount: 2</c>.
    /// Kỳ vọng: chỉ xử lý/import 2 dòng đầu (ValidRowCount = ProcessedRowCount = 2).
    /// </summary>
    [Fact]
    public async Task Case2_Import_RowCountLimitsToFirstNRows()
    {
        await using var file = CustomerTestData.SampleCustomersWorkbook();
        var result = await ProcessorTestFactory.CreateImport().ImportAsync(
            ImportWorkFactory.Create(file, ImportWorkFactory.Template(rowCount: 2)),
            CancellationToken.None);

        Assert.Equal(2, result.ValidRowCount);
        Assert.Equal(2, result.ProcessedRowCount);
    }

    /// <summary>
    /// Case 3 — Sai tên sheet trong template.
    /// Setup: template trỏ sheet <c>CustomerABC</c> trong khi file chỉ có sheet chuẩn.
    /// Kỳ vọng: ném <see cref="InvalidTemplateException"/>, message chứa tên sheet và "was not found".
    /// </summary>
    [Fact]
    public async Task Case3_Import_WrongSheetName_ThrowsSheetNotFound()
    {
        await using var file = CustomerTestData.SampleCustomersWorkbook();
        var ex = await Assert.ThrowsAsync<InvalidTemplateException>(() =>
            ProcessorTestFactory.CreateImport().ImportAsync(
                ImportWorkFactory.Create(file, ImportWorkFactory.Template(sheetName: "CustomerABC")),
                CancellationToken.None));

        Assert.Contains("CustomerABC", ex.Message);
        Assert.Contains("was not found", ex.Message);
    }

    /// <summary>
    /// Case 4 — Sai vị trí header row.
    /// Setup: <c>headerRow: 2</c>, <c>startRow: 3</c> nên engine đọc nhầm dòng data làm header → không tìm thấy cột.
    /// Kỳ vọng: ném <see cref="InvalidTemplateException"/> (column not found).
    /// </summary>
    [Fact]
    public async Task Case4_Import_WrongHeaderRow_ThrowsColumnNotFound()
    {
        await using var file = CustomerTestData.SampleCustomersWorkbook();
        var ex = await Assert.ThrowsAsync<InvalidTemplateException>(() =>
            ProcessorTestFactory.CreateImport().ImportAsync(
                ImportWorkFactory.Create(file, ImportWorkFactory.Template(headerRow: 2, startRow: 3)),
                CancellationToken.None));

        Assert.Contains("not found", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Case 5 — Mapping một phần (chỉ map một số cột).
    /// Setup: chỉ map <c>CustomerCode</c> và <c>Phone</c> (Phone required).
    /// Kỳ vọng: import thành công 3 dòng; mỗi row chỉ có 2 field đã map, không có <c>CustomerName</c>.
    /// </summary>
    [Fact]
    public async Task Case5_Import_PartialMappings_ReturnsOnlyMappedFields()
    {
        await using var file = CustomerTestData.SampleCustomersWorkbook();
        var template = ImportWorkFactory.Template(mappings:
        [
            new ColumnMapping("CustomerCode", "CustomerCode"),
            new ColumnMapping("Phone", "Phone", Required: true)
        ]);

        var result = await ProcessorTestFactory.CreateImport().ImportAsync(
            ImportWorkFactory.Create(file, template),
            CancellationToken.None);

        Assert.Equal(3, result.ValidRowCount);
        var row = result.Sheets[0].Rows[0];
        Assert.Equal(2, row.Count);
        Assert.True(row.ContainsKey("CustomerCode"));
        Assert.True(row.ContainsKey("Phone"));
        Assert.False(row.ContainsKey("CustomerName"));
    }

    /// <summary>
    /// Case 6 — Tên cột Excel trong mapping không khớp file.
    /// Setup: map <c>CustomerCodeXXX</c> (không tồn tại trên header).
    /// Kỳ vọng: ném <see cref="InvalidTemplateException"/> với message <c>ColumnNotFound</c>.
    /// </summary>
    [Fact]
    public async Task Case6_Import_WrongExcelColumnName_ThrowsColumnNotFound()
    {
        await using var file = CustomerTestData.SampleCustomersWorkbook();
        var template = ImportWorkFactory.Template(mappings:
        [
            new ColumnMapping("CustomerCodeXXX", "CustomerCode", Required: true),
            new ColumnMapping("CustomerName", "CustomerName")
        ]);

        var ex = await Assert.ThrowsAsync<InvalidTemplateException>(() =>
            ProcessorTestFactory.CreateImport().ImportAsync(
                ImportWorkFactory.Create(file, template),
                CancellationToken.None));

        Assert.Equal(EngineMessages.ColumnNotFound("CustomerCodeXXX"), ex.Message);
    }

    /// <summary>
    /// Case 7 — Dòng trống xen giữa dữ liệu được bỏ qua.
    /// Setup: CUS001 → row trống toàn null → CUS003.
    /// Kỳ vọng: ValidRowCount = 2; thứ tự kết quả là CUS001 rồi CUS003 (không fail vì blank row).
    /// </summary>
    [Fact]
    public async Task Case7_Import_BlankRowsInMiddle_AreSkipped()
    {
        var rows = new List<Dictionary<string, object?>>
        {
            CustomerTestData.ExcelRow("CUS001", "A", "a@example.com", "0901", "1990-01-01"),
            new()
            {
                ["CustomerCode"] = null,
                ["CustomerName"] = null,
                ["Email"] = null,
                ["Phone"] = null,
                ["BirthDate"] = null
            },
            CustomerTestData.ExcelRow("CUS003", "C", "c@example.com", "0903", "1992-01-01")
        };
        await using var file = ExcelWorkbookFactory.Create(rows);

        var result = await ProcessorTestFactory.CreateImport().ImportAsync(
            ImportWorkFactory.Create(file, ImportWorkFactory.Template()),
            CancellationToken.None);

        Assert.Equal(2, result.ValidRowCount);
        Assert.Equal("CUS001", result.Sheets[0].Rows[0]["CustomerCode"]?.ToString());
        Assert.Equal("CUS003", result.Sheets[0].Rows[1]["CustomerCode"]?.ToString());
    }

    /// <summary>
    /// Case 8 — Đổi tên field output qua mapping (<c>ColumnName</c> → <c>FieldName</c>).
    /// Setup: map sang key <c>Code</c>, <c>Name</c>, <c>Mail</c>, <c>Mobile</c>, <c>DOB</c>.
    /// Kỳ vọng: dictionary kết quả dùng FieldName làm key, không còn key Excel gốc.
    /// </summary>
    [Fact]
    public async Task Case8_Import_RenamedFieldNames_UsesFieldNameAsKeys()
    {
        await using var file = CustomerTestData.SampleCustomersWorkbook();
        var template = ImportWorkFactory.Template(mappings:
        [
            new("CustomerCode", "Code"),
            new("CustomerName", "Name"),
            new("Email", "Mail"),
            new("Phone", "Mobile"),
            new("BirthDate", "DOB")
        ]);

        var result = await ProcessorTestFactory.CreateImport().ImportAsync(
            ImportWorkFactory.Create(file, template),
            CancellationToken.None);

        var row = result.Sheets[0].Rows[0];
        Assert.Equal("CUS001", row["Code"]?.ToString());
        Assert.Equal("Nguyen Van A", row["Name"]?.ToString());
        Assert.True(row.ContainsKey("Mail"));
        Assert.True(row.ContainsKey("Mobile"));
        Assert.True(row.ContainsKey("DOB"));
        Assert.False(row.ContainsKey("CustomerCode"));
    }

    /// <summary>
    /// Case 9 — <c>StartRow</c> bỏ qua dòng data đầu.
    /// Setup: header row 1, startRow = 3 → bỏ CUS001 (ở row 2), bắt đầu từ CUS002.
    /// Kỳ vọng: ValidRowCount = 2; dòng đầu kết quả là CUS002 rồi CUS003.
    /// </summary>
    [Fact]
    public async Task Case9_Import_StartRowSkipsFirstDataRow()
    {
        await using var file = CustomerTestData.SampleCustomersWorkbook();
        var result = await ProcessorTestFactory.CreateImport().ImportAsync(
            ImportWorkFactory.Create(file, ImportWorkFactory.Template(startRow: 3)),
            CancellationToken.None);

        Assert.Equal(2, result.ValidRowCount);
        Assert.Equal("CUS002", result.Sheets[0].Rows[0]["CustomerCode"]?.ToString());
        Assert.Equal("CUS003", result.Sheets[0].Rows[1]["CustomerCode"]?.ToString());
    }

    /// <summary>
    /// Case 10 — <see cref="InvalidRowBehavior.Stop"/>: dừng khi gặp dòng lỗi đầu tiên.
    /// Setup: CUS001 hợp lệ → dòng thiếu CustomerCode (required) → CUS003 hợp lệ.
    /// Kỳ vọng: có failed ≥ 1; chỉ giữ 1 dòng valid (CUS001); CUS003 không xuất hiện trong kết quả.
    /// </summary>
    [Fact]
    public async Task Case10_Import_InvalidRowBehaviorStop_StopsAtFirstError()
    {
        await using var file = CreateFileWithInvalidMiddleRow();
        var template = ImportWorkFactory.Template(
            invalidRowBehavior: InvalidRowBehavior.Stop,
            mappings: CustomerTestData.EnglishMappings());

        var result = await ProcessorTestFactory.CreateImport().ImportAsync(
            ImportWorkFactory.Create(file, template),
            CancellationToken.None);

        Assert.True(result.FailedRowCount >= 1);
        Assert.Equal(1, result.ValidRowCount);
        Assert.DoesNotContain(result.Sheets[0].Rows, r => r["CustomerCode"]?.ToString() == "CUS003");
    }

    /// <summary>
    /// Case 11 — <see cref="InvalidRowBehavior.Continue"/>: bỏ qua dòng lỗi, tiếp tục các dòng sau.
    /// Setup: cùng file Case 10 (dòng giữa invalid).
    /// Kỳ vọng: FailedRowCount = 1; ValidRowCount = 2 (CUS001 + CUS003).
    /// </summary>
    [Fact]
    public async Task Case11_Import_InvalidRowBehaviorContinue_SkipsBadRows()
    {
        await using var file = CreateFileWithInvalidMiddleRow();
        var template = ImportWorkFactory.Template(
            invalidRowBehavior: InvalidRowBehavior.Continue,
            mappings: CustomerTestData.EnglishMappings());

        var result = await ProcessorTestFactory.CreateImport().ImportAsync(
            ImportWorkFactory.Create(file, template),
            CancellationToken.None);

        Assert.Equal(1, result.FailedRowCount);
        Assert.Equal(2, result.ValidRowCount);
        Assert.Equal("CUS001", result.Sheets[0].Rows[0]["CustomerCode"]?.ToString());
        Assert.Equal("CUS003", result.Sheets[0].Rows[1]["CustomerCode"]?.ToString());
    }

    /// <summary>
    /// Workbook 3 dòng: valid → invalid (thiếu CustomerCode nhưng còn field khác) → valid.
    /// Dùng chung cho Case 10/11 kiểm tra InvalidRowBehavior.
    /// </summary>
    private static MemoryStream CreateFileWithInvalidMiddleRow() =>
        ExcelWorkbookFactory.Create(
        [
            CustomerTestData.ExcelRow("CUS001", "A", "a@example.com", "0901", "1990-01-01"),
            new()
            {
                ["CustomerCode"] = null,
                ["CustomerName"] = "Missing code",
                ["Email"] = "x@example.com",
                ["Phone"] = "0900",
                ["BirthDate"] = "1991-01-01"
            },
            CustomerTestData.ExcelRow("CUS003", "C", "c@example.com", "0903", "1992-01-01")
        ]);
}
