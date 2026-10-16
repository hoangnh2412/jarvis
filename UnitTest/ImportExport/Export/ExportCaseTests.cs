namespace UnitTest.ImportExport.Export;

/// <summary>
/// Các case export end-to-end (từ legacy note).
/// Payload dùng <see cref="ExportWork"/> hiện tại; mapping mặc định là header tiếng Việt.
/// </summary>
public class ExportCaseTests
{
    private static readonly ColumnMapping[] DefaultMappings = CustomerTestData.VietnameseExportMappings();

    /// <summary>
    /// Case 1 — Happy path: export đủ cột đã map.
    /// Setup: 3 record đầy đủ + mapping tiếng Việt.
    /// Kỳ vọng: file có content; sheet Customers có 3 dòng; giá trị cột header VN đúng.
    /// </summary>
    [Fact]
    public async Task Case1_Export_Success_WritesAllMappedColumns()
    {
        var data = new[]
        {
            CustomerTestData.Row("CUS001", "Nguyen Van An", "an@example.com", "0901234567"),
            CustomerTestData.Row("CUS002", "Tran Thi Be", "be@example.com", "0912345678"),
            CustomerTestData.Row("CUS003", "Le Van Cong", "cong@example.com", "0923456789")
        };

        var result = await ProcessorTestFactory.CreateExport().ExportAsync(
            ExportWorkFactory.Create(data, DefaultMappings, "CustomerExport.xlsx", "Customers"),
            CancellationToken.None);

        Assert.True(result.Content.Length > 0);
        var rows = ExcelWorkbookFactory.ReadSheet(result.Content, "Customers");
        Assert.Equal(3, rows.Count);
        Assert.Equal("CUS001", rows[0]["Mã Khách Hàng"]?.ToString());
        Assert.Equal("Nguyen Van An", rows[0]["Tên Khách Hàng"]?.ToString());
    }

    /// <summary>
    /// Case 2 — Record thiếu field so với mapping.
    /// Setup: mỗi record chỉ có <c>CustomerCode</c>, mapping vẫn yêu cầu Name/Email/Phone.
    /// Kỳ vọng: vẫn tạo đủ 2 dòng; các cột thiếu ghi ô trống (blank).
    /// </summary>
    [Fact]
    public async Task Case2_Export_PartialRecords_MissingFieldsAreEmpty()
    {
        var data = new[]
        {
            new Dictionary<string, object?> { ["CustomerCode"] = "CUS001" },
            new Dictionary<string, object?> { ["CustomerCode"] = "CUS002" }
        };

        var result = await ProcessorTestFactory.CreateExport().ExportAsync(
            ExportWorkFactory.Create(data, DefaultMappings, "CustomerExport.xlsx", "Customers"),
            CancellationToken.None);

        var rows = ExcelWorkbookFactory.ReadSheet(result.Content, "Customers");
        Assert.Equal(2, rows.Count);
        Assert.Equal("CUS001", rows[0]["Mã Khách Hàng"]?.ToString());
        Assert.True(CustomerTestData.IsBlank(rows[0]["Tên Khách Hàng"]));
        Assert.True(CustomerTestData.IsBlank(rows[0]["Email"]));
        Assert.True(CustomerTestData.IsBlank(rows[0]["Số Điện Thoại"]));
    }

    /// <summary>
    /// Case 3 — Custom header: <c>ColumnName</c> trở thành tên cột trên Excel.
    /// Setup: map <c>Code</c>/<c>Full Name</c> thay vì tên field nội bộ.
    /// Kỳ vọng: sheet dùng header custom; không còn key <c>CustomerCode</c>.
    /// </summary>
    [Fact]
    public async Task Case3_Export_CustomHeaderNames_UsesColumnNameAsHeader()
    {
        var mappings = new[]
        {
            new ColumnMapping("Code", "CustomerCode", Required: true),
            new ColumnMapping("Full Name", "CustomerName", Required: true)
        };
        var data = new[]
        {
            CustomerTestData.Row("CUS001", "Nguyen Van An")
        };

        var result = await ProcessorTestFactory.CreateExport().ExportAsync(
            ExportWorkFactory.Create(data, mappings, "Headers.xlsx", "Customers"),
            CancellationToken.None);

        var rows = ExcelWorkbookFactory.ReadSheet(result.Content, "Customers");
        Assert.Single(rows);
        Assert.True(rows[0].ContainsKey("Code"));
        Assert.True(rows[0].ContainsKey("Full Name"));
        Assert.False(rows[0].ContainsKey("CustomerCode"));
    }

    /// <summary>
    /// Case 4 — Mapping rỗng không hợp lệ.
    /// Setup: <c>ColumnMapping[]</c> empty.
    /// Kỳ vọng: ném <see cref="InvalidTemplateException"/> với <c>MappingsRequired</c>.
    /// </summary>
    [Fact]
    public async Task Case4_Export_MappingsEmpty_ThrowsMappingsRequired()
    {
        var ex = await Assert.ThrowsAsync<InvalidTemplateException>(() =>
            ProcessorTestFactory.CreateExport().ExportAsync(
                ExportWorkFactory.Create(
                    [new Dictionary<string, object?> { ["CustomerCode"] = "CUS001" }],
                    Array.Empty<ColumnMapping>(),
                    "EmptyMap.xlsx",
                    "Customers"),
                CancellationToken.None));

        Assert.Equal(EngineMessages.MappingsRequired, ex.Message);
    }

    /// <summary>
    /// Case 5 — Trùng <c>FieldName</c> trong mapping.
    /// Setup: hai cột khác header nhưng cùng FieldName <c>CustomerCode</c>.
    /// Kỳ vọng: ném <see cref="InvalidTemplateException"/> với <c>FieldNameUnique</c>.
    /// </summary>
    [Fact]
    public async Task Case5_Export_DuplicateFieldName_Throws()
    {
        var mappings = new[]
        {
            new ColumnMapping("Mã Khách Hàng", "CustomerCode", Required: true),
            new ColumnMapping("Mã KH", "CustomerCode", Required: true)
        };

        var ex = await Assert.ThrowsAsync<InvalidTemplateException>(() =>
            ProcessorTestFactory.CreateExport().ExportAsync(
                ExportWorkFactory.Create(
                    [CustomerTestData.Row("CUS001", "A", "a@b.com", "1")],
                    mappings,
                    "DupField.xlsx",
                    "Customers"),
                CancellationToken.None));

        Assert.Equal(EngineMessages.FieldNameUnique, ex.Message);
    }

    /// <summary>
    /// Case 6 — Trùng <c>ColumnName</c> (header Excel) trong mapping.
    /// Setup: hai field khác nhau nhưng cùng header <c>Mã Khách Hàng</c>.
    /// Kỳ vọng: ném <see cref="InvalidTemplateException"/> với <c>ColumnNameUnique</c>.
    /// </summary>
    [Fact]
    public async Task Case6_Export_DuplicateColumnName_Throws()
    {
        var mappings = new[]
        {
            new ColumnMapping("Mã Khách Hàng", "CustomerCode", Required: true),
            new ColumnMapping("Mã Khách Hàng", "CustomerName", Required: true)
        };

        var ex = await Assert.ThrowsAsync<InvalidTemplateException>(() =>
            ProcessorTestFactory.CreateExport().ExportAsync(
                ExportWorkFactory.Create(
                    [CustomerTestData.Row("CUS001", "A", "a@b.com", "1")],
                    mappings,
                    "DupCol.xlsx",
                    "Customers"),
                CancellationToken.None));

        Assert.Equal(EngineMessages.ColumnNameUnique, ex.Message);
    }

    /// <summary>
    /// Case 7 — Giới hạn số bản ghi bằng <c>RowCount</c>.
    /// Setup: 3 record nhưng <c>rowCount: 2</c>.
    /// Kỳ vọng: chỉ export 2 bản ghi đầu (CUS001, CUS002).
    /// </summary>
    [Fact]
    public async Task Case7_Export_RowCountLimitsToFirstNRecords()
    {
        var data = new[]
        {
            CustomerTestData.Row("CUS001", "Nguyen Van An", "an@example.com", "0901234567"),
            CustomerTestData.Row("CUS002", "Tran Thi Be", "be@example.com", "0912345678"),
            CustomerTestData.Row("CUS003", "Le Van Cong", "cong@example.com", "0923456789")
        };

        var result = await ProcessorTestFactory.CreateExport().ExportAsync(
            ExportWorkFactory.Create(
                data,
                DefaultMappings,
                "CustomerExport.xlsx",
                "Customers",
                rowCount: 2),
            CancellationToken.None);

        var rows = ExcelWorkbookFactory.ReadSheet(result.Content, "Customers");
        Assert.Equal(2, rows.Count);
        Assert.Equal("CUS001", rows[0]["Mã Khách Hàng"]?.ToString());
        Assert.Equal("CUS002", rows[1]["Mã Khách Hàng"]?.ToString());
    }

    /// <summary>
    /// Case 8 — Data rỗng vẫn tạo file Excel hợp lệ.
    /// Setup: mảng data empty + mapping đầy đủ.
    /// Kỳ vọng: file có content, đuôi .xlsx; sheet không có dòng data (chỉ header).
    /// </summary>
    [Fact]
    public async Task Case8_Export_EmptyData_CreatesFileWithNoDataRows()
    {
        var result = await ProcessorTestFactory.CreateExport().ExportAsync(
            ExportWorkFactory.Create(
                Array.Empty<Dictionary<string, object?>>(),
                DefaultMappings,
                "Empty.xlsx",
                "Customers"),
            CancellationToken.None);

        Assert.True(result.Content.Length > 0);
        Assert.EndsWith(".xlsx", result.FileName, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(ExcelWorkbookFactory.ReadSheet(result.Content, "Customers"));
    }

    /// <summary>
    /// Case 9 — Giá trị null / field null vẫn export thành công.
    /// Setup: CustomerCode = null, Email = null, CustomerName có giá trị.
    /// Kỳ vọng: ô tương ứng blank; Name được ghi đúng.
    /// </summary>
    [Fact]
    public async Task Case9_Export_NullAndMissingFields_SucceedsWithBlankCells()
    {
        var mappings = new[]
        {
            new ColumnMapping("Mã KH", "CustomerCode", Required: true),
            new ColumnMapping("Tên KH", "CustomerName", Required: true),
            new ColumnMapping("Email", "Email")
        };
        var data = new[]
        {
            new Dictionary<string, object?>
            {
                ["CustomerCode"] = null,
                ["CustomerName"] = "Nguyen Van An",
                ["Email"] = null
            }
        };

        var result = await ProcessorTestFactory.CreateExport().ExportAsync(
            ExportWorkFactory.Create(data, mappings, "NullValue.xlsx", "Customers"),
            CancellationToken.None);

        var rows = ExcelWorkbookFactory.ReadSheet(result.Content, "Customers");
        Assert.Single(rows);
        Assert.True(CustomerTestData.IsBlank(rows[0]["Mã KH"]));
        Assert.Equal("Nguyen Van An", rows[0]["Tên KH"]?.ToString());
        Assert.True(CustomerTestData.IsBlank(rows[0]["Email"]));
    }

    /// <summary>
    /// Case 11 — Chỉ định <c>SheetName</c> khi export.
    /// Setup: sheetName = <c>Customers</c>.
    /// Kỳ vọng: workbook chứa sheet đúng tên đó.
    /// </summary>
    [Fact]
    public async Task Case11_Export_SheetName_CreatesNamedSheet()
    {
        var result = await ProcessorTestFactory.CreateExport().ExportAsync(
            ExportWorkFactory.Create(
                [CustomerTestData.Row("CUS001", "A")],
                DefaultMappings,
                "Named.xlsx",
                "Customers"),
            CancellationToken.None);

        Assert.Contains("Customers", ExcelWorkbookFactory.GetSheetNames(result.Content));
    }

    /// <summary>
    /// Case 12 — Không truyền <c>SheetName</c> (null).
    /// Setup: sheetName = null.
    /// Kỳ vọng: dùng tên sheet mặc định của engine (<c>DefaultExportSheetName</c>).
    /// </summary>
    [Fact]
    public async Task Case12_Export_SheetNameNull_UsesDefaultSheet()
    {
        var mappings = new[] { new ColumnMapping("Mã KH", "CustomerCode", Required: true) };
        var data = new[] { new Dictionary<string, object?> { ["CustomerCode"] = "CUS001" } };

        var result = await ProcessorTestFactory.CreateExport().ExportAsync(
            ExportWorkFactory.Create(data, mappings, "DefaultSheet.xlsx", sheetName: null),
            CancellationToken.None);

        Assert.Contains(EngineMessages.DefaultExportSheetName, ExcelWorkbookFactory.GetSheetNames(result.Content));
    }
}
