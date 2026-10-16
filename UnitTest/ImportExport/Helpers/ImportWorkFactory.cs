namespace UnitTest.ImportExport.Helpers;

public static class ImportWorkFactory
{
    public static TemplateConfiguration Template(
        string sheetName = "Customers",
        int headerRow = 1,
        int startRow = 2,
        int? rowCount = null,
        InvalidRowBehavior invalidRowBehavior = InvalidRowBehavior.Continue,
        IReadOnlyCollection<ColumnMapping>? mappings = null) =>
        new()
        {
            SheetName = sheetName,
            HeaderRow = headerRow,
            StartRow = startRow,
            RowCount = rowCount,
            InvalidRowBehavior = invalidRowBehavior,
            Mappings = mappings ?? CustomerTestData.EnglishMappings()
        };

    public static ImportWork Create(
        Stream file,
        TemplateConfiguration template,
        string fileName = "customers.xlsx",
        string? templateCode = "CUSTOMER_DEMO",
        ImportExecutionPhase phase = ImportExecutionPhase.Import) =>
        new()
        {
            FileStream = file,
            FileName = fileName,
            Template = template,
            TemplateCode = templateCode,
            TemplateFileName = fileName,
            Phase = phase
        };
}
