using Microsoft.Extensions.Logging.Abstractions;
using Platform.Modules.ImportExport.EntityFramework.Services;

namespace UnitTest.ImportExport.Infrastructure;

/// <summary>CRUD cases for <see cref="TemplateService"/> with in-memory store (no DB / HTTP).</summary>
public class TemplateServiceTests
{
    private static readonly byte[] SampleContent = [0x50, 0x4B, 0x03, 0x04]; // zip/xlsx magic

    private static ColumnMapping[] DefaultMappings() =>
    [
        new("CustomerCode", "CustomerCode", Required: true),
        new("CustomerName", "CustomerName")
    ];

    private static CreateTemplateRequest CreateRequest(
        string code = "CUSTOMER_IMPORT",
        string name = "Customer Import",
        string fileName = "customer.xlsx",
        byte[]? content = null,
        IReadOnlyList<ColumnMapping>? mappings = null,
        IReadOnlyList<SheetTemplate>? sheets = null,
        int headerRow = 1,
        int startRow = 2,
        int? rowCount = null,
        string? sheetName = "Sheet1") =>
        new()
        {
            Code = code,
            Name = name,
            FileName = fileName,
            Content = content ?? SampleContent,
            Mappings = mappings ?? DefaultMappings(),
            Sheets = sheets ?? [],
            HeaderRow = headerRow,
            StartRow = startRow,
            RowCount = rowCount,
            SheetName = sheetName
        };

    private static TemplateService CreateSut(out InMemoryTemplateStore store)
    {
        store = new InMemoryTemplateStore();
        return new TemplateService(store, NullLogger<TemplateService>.Instance);
    }

    [Fact]
    public async Task Case1_Create_HappyPath_ReturnsDetailAndPersists()
    {
        var sut = CreateSut(out var store);

        var created = await sut.CreateAsync(CreateRequest());

        Assert.Equal("CUSTOMER_IMPORT", created.Code);
        Assert.Equal("Customer Import", created.Name);
        Assert.Equal("customer.xlsx", created.FileName);
        Assert.Equal(SampleContent, created.Content);
        Assert.Equal(2, created.Mappings.Count);

        var fromStore = await store.GetByCodeAsync("CUSTOMER_IMPORT");
        Assert.NotNull(fromStore);
        Assert.Equal("CUSTOMER_IMPORT", fromStore!.Code);
    }

    [Fact]
    public async Task Case2_Create_EmptyContent_ThrowsInvalidTemplate()
    {
        var sut = CreateSut(out _);

        var ex = await Assert.ThrowsAsync<InvalidTemplateException>(() =>
            sut.CreateAsync(CreateRequest(content: [])));

        Assert.Equal(EngineMessages.TemplateContentRequired, ex.Message);
    }

    [Fact]
    public async Task Case3_Create_EmptyMappings_ThrowsInvalidTemplate()
    {
        var sut = CreateSut(out _);

        var ex = await Assert.ThrowsAsync<InvalidTemplateException>(() =>
            sut.CreateAsync(CreateRequest(mappings: [])));

        Assert.Equal(EngineMessages.MappingsRequired, ex.Message);
    }

    [Fact]
    public async Task Case4_Create_MultiSheetMissingSheetName_ThrowsInvalidTemplate()
    {
        var sut = CreateSut(out _);
        var sheets = new[]
        {
            new SheetTemplate { SheetName = null, Mappings = DefaultMappings() },
            new SheetTemplate { SheetName = "Orders", Mappings = DefaultMappings() }
        };

        var ex = await Assert.ThrowsAsync<InvalidTemplateException>(() =>
            sut.CreateAsync(CreateRequest(mappings: [], sheets: sheets)));

        Assert.Equal(EngineMessages.SheetNameRequiredForMultiSheet, ex.Message);
    }

    [Fact]
    public async Task Case5_Create_PersistsHeaderStartRowAndRowCount()
    {
        var sut = CreateSut(out _);

        var created = await sut.CreateAsync(CreateRequest(
            headerRow: 2,
            startRow: 3,
            rowCount: 100,
            sheetName: "Customers"));

        Assert.Equal(2, created.HeaderRow);
        Assert.Equal(3, created.StartRow);
        Assert.Equal(100, created.RowCount);
        Assert.Equal("Customers", created.SheetName);
    }

    [Fact]
    public async Task Case6_Get_Existing_ReturnsDetail()
    {
        var sut = CreateSut(out _);
        await sut.CreateAsync(CreateRequest(code: "T_GET", name: "Get Me"));

        var detail = await sut.GetAsync("T_GET");

        Assert.Equal("T_GET", detail.Code);
        Assert.Equal("Get Me", detail.Name);
        Assert.Equal(SampleContent, detail.Content);
    }

    [Fact]
    public async Task Case7_Get_Missing_ThrowsTemplateNotFound()
    {
        var sut = CreateSut(out _);

        await Assert.ThrowsAsync<TemplateNotFoundException>(() =>
            sut.GetAsync("MISSING_CODE"));
    }

    [Fact]
    public async Task Case8_GetForExecution_ReturnsSummaryWithoutNeedingSeparateContentAssert()
    {
        var sut = CreateSut(out _);
        await sut.CreateAsync(CreateRequest(code: "T_EXEC", name: "Exec"));

        var summary = await sut.GetForExecutionAsync("T_EXEC");

        Assert.Equal("T_EXEC", summary.Code);
        Assert.Equal("Exec", summary.Name);
        Assert.Equal(2, summary.Mappings.Count);
        Assert.IsNotType<TemplateDetail>(summary);
    }

    [Fact]
    public async Task Case9_List_EmptyThenMultipleItems()
    {
        var sut = CreateSut(out _);

        Assert.Empty(await sut.ListAsync());

        await sut.CreateAsync(CreateRequest(code: "A_ONE", name: "One"));
        await sut.CreateAsync(CreateRequest(code: "B_TWO", name: "Two"));

        var list = await sut.ListAsync();
        Assert.Equal(2, list.Count);
        Assert.Contains(list, x => x.Code == "A_ONE");
        Assert.Contains(list, x => x.Code == "B_TWO");
    }

    [Fact]
    public async Task Case10_Update_ChangesName()
    {
        var sut = CreateSut(out _);
        await sut.CreateAsync(CreateRequest(code: "T_UPD", name: "Old"));

        var updated = await sut.UpdateMetadataAsync("T_UPD", new UpdateTemplateMetadataRequest { Name = "New Name" });

        Assert.Equal("New Name", updated.Name);
        Assert.Equal("customer.xlsx", updated.FileName);
        Assert.Equal(SampleContent, updated.Content);
    }

    [Fact]
    public async Task Case11_Update_ReplacesContentAndFileName()
    {
        var sut = CreateSut(out _);
        await sut.CreateAsync(CreateRequest(code: "T_FILE"));
        var newBytes = new byte[] { 1, 2, 3, 4, 5 };

        var updated = await sut.UpdateAsync("T_FILE", new UpdateTemplateRequest
        {
            FileName = "v2.xlsx",
            Content = newBytes
        });

        Assert.Equal("v2.xlsx", updated.FileName);
        Assert.Equal(newBytes, updated.Content);
    }

    [Fact]
    public async Task Case12_Update_ReplacesMappings()
    {
        var sut = CreateSut(out _);
        await sut.CreateAsync(CreateRequest(code: "T_MAP"));
        var newMappings = new[]
        {
            new ColumnMapping("Code", "Code", Required: true),
            new ColumnMapping("Email", "Email", Unique: true)
        };

        var updated = await sut.UpdateAsync("T_MAP", new UpdateTemplateRequest { Mappings = newMappings });

        Assert.Equal(2, updated.Mappings.Count);
        Assert.Equal("Code", updated.Mappings[0].FieldName);
        Assert.True(updated.Mappings[1].Unique);
    }

    [Fact]
    public async Task Case13_Update_MissingCode_ThrowsTemplateNotFound()
    {
        var sut = CreateSut(out _);

        await Assert.ThrowsAsync<TemplateNotFoundException>(() =>
            sut.UpdateAsync("NOPE", new UpdateTemplateRequest { Name = "X" }));
    }

    [Fact]
    public async Task Case14_Delete_Existing_RemovesTemplate()
    {
        var sut = CreateSut(out _);
        await sut.CreateAsync(CreateRequest(code: "T_DEL"));

        await sut.DeleteAsync("T_DEL");

        await Assert.ThrowsAsync<TemplateNotFoundException>(() => sut.GetAsync("T_DEL"));
        Assert.Empty(await sut.ListAsync());
    }

    [Fact]
    public async Task Case15_Delete_Missing_ThrowsTemplateNotFound()
    {
        var sut = CreateSut(out _);

        await Assert.ThrowsAsync<TemplateNotFoundException>(() =>
            sut.DeleteAsync("GONE"));
    }
}
