namespace UnitTest.ImportExport.Infrastructure;

/// <summary>
/// Unit test cho <c>TemplateConfiguration.ResolveSheetPlans()</c> —
/// chuẩn hóa cấu hình legacy (1 sheet) và multi-sheet thành danh sách SheetPlan.
/// </summary>
public class SheetPlanTests
{
    /// <summary>
    /// Legacy: dùng top-level <c>Mappings</c> + <c>SheetName</c>.
    /// Kỳ vọng: đúng 1 plan, SheetName = "Customers".
    /// </summary>
    [Fact]
    public void ResolveSheetPlans_LegacyMappings_SinglePlan()
    {
        var cfg = new TemplateConfiguration
        {
            Mappings = [new ColumnMapping("A", "A")],
            SheetName = "Customers"
        };

        var plans = cfg.ResolveSheetPlans();
        Assert.Single(plans);
        Assert.Equal("Customers", plans[0].SheetName);
    }

    /// <summary>
    /// Multi-sheet: dùng danh sách <c>Sheets</c>.
    /// Kỳ vọng: số plan = số sheet (Customers + Orders = 2).
    /// </summary>
    [Fact]
    public void ResolveSheetPlans_MultiSheet_UsesSheets()
    {
        var cfg = new TemplateConfiguration
        {
            Sheets =
            [
                new SheetTemplate { SheetName = "Customers", Mappings = [new ColumnMapping("A", "A")] },
                new SheetTemplate { SheetName = "Orders", Mappings = [new ColumnMapping("B", "B")] }
            ]
        };

        Assert.Equal(2, cfg.ResolveSheetPlans().Count);
    }
}
