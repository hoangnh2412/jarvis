using Platform.ImportExport.MiniExcel.Processors;
using Platform.ImportExport.MiniExcel.Validation;
using Platform.Modules.ImportExport.EntityFramework.Stores;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace UnitTest.ImportExport.Helpers;

/// <summary>Shared DI-style construction for processor unit tests (no host).</summary>
public static class ProcessorTestFactory
{
    public static ExcelEngineOptions EngineOptions { get; } = new()
    {
        ImportTimeoutSeconds = 0,
        ExportTimeoutSeconds = 0,
        MaxRowCount = 100_000,
        MaxColumnCount = 200,
        PersistBatchSize = 100,
        IncludeRowsInResponse = true,
        IncludeRowsMaxRowCount = 100_000
    };

    public static IOptions<ExcelEngineOptions> OptionsSnapshot { get; } =
        Options.Create(EngineOptions);

    public static MiniExcelExportProcessor CreateExport() =>
        new(OptionsSnapshot, NullLogger<MiniExcelExportProcessor>.Instance);

    public static MiniExcelImportProcessor CreateImport() =>
        new(
            new ImportRequestValidator(OptionsSnapshot),
            new DefaultRowValidator(new DefaultUniqueValueChecker()),
            new DefaultImportDataPersister(NullLogger<DefaultImportDataPersister>.Instance),
            OptionsSnapshot,
            NullLogger<MiniExcelImportProcessor>.Instance);
}
