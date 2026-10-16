namespace UnitTest.ImportExport.Helpers;

/// <summary>Shared Customer sample rows / mappings used by Import &amp; Export cases.</summary>
public static class CustomerTestData
{
    public static readonly string[] Headers =
        ["CustomerCode", "CustomerName", "Email", "Phone", "BirthDate"];

    public static ColumnMapping[] EnglishMappings(
        bool phoneRequired = false) =>
    [
        new("CustomerCode", "CustomerCode", Required: true),
        new("CustomerName", "CustomerName", Required: true),
        new("Email", "Email"),
        new("Phone", "Phone", Required: phoneRequired),
        new("BirthDate", "BirthDate")
    ];

    public static ColumnMapping[] VietnameseExportMappings() =>
    [
        new("Mã Khách Hàng", "CustomerCode", Required: true),
        new("Tên Khách Hàng", "CustomerName", Required: true),
        new("Email", "Email"),
        new("Số Điện Thoại", "Phone")
    ];

    public static Dictionary<string, object?> Row(
        string code,
        string name,
        string? email = null,
        string? phone = null,
        string? birthDate = null) =>
        new()
        {
            ["CustomerCode"] = code,
            ["CustomerName"] = name,
            ["Email"] = email,
            ["Phone"] = phone,
            ["BirthDate"] = birthDate
        };

    public static Dictionary<string, object?> ExcelRow(
        string code,
        string name,
        string email,
        string phone,
        string birth) =>
        new()
        {
            [Headers[0]] = code,
            [Headers[1]] = name,
            [Headers[2]] = email,
            [Headers[3]] = phone,
            [Headers[4]] = birth
        };

    public static MemoryStream SampleCustomersWorkbook(string sheetName = "Customers") =>
        ExcelWorkbookFactory.Create(
        [
            ExcelRow("CUS001", "Nguyen Van A", "a@example.com", "0901234567", "1990-01-01"),
            ExcelRow("CUS002", "Tran Thi B", "b@example.com", "0912345678", "1992-05-10"),
            ExcelRow("CUS003", "Le Van C", "c@example.com", "0923456789", "1988-12-20")
        ],
        sheetName);

    public static MemoryStream SampleCustomersCsv()
    {
        var content =
            """
            CustomerCode,CustomerName,Email,Phone,BirthDate
            CUS001,Nguyen Van A,a@example.com,0901234567,1990-01-01
            CUS002,Tran Thi B,b@example.com,0912345678,1992-05-10
            CUS003,Le Van C,c@example.com,0923456789,1988-12-20
            """;
        return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
    }

    public static bool IsBlank(object? value) =>
        value is null || (value is string s && string.IsNullOrWhiteSpace(s));
}
