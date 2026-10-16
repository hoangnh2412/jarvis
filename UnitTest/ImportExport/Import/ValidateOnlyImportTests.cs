using Platform.ImportExport.MiniExcel.Processors;
using Platform.ImportExport.MiniExcel.Validation;
using Platform.Modules.ImportExport.EntityFramework.Stores;
using Microsoft.Extensions.Logging.Abstractions;

namespace UnitTest.ImportExport.Import;

public class ValidateOnlyImportTests
{
    private sealed class CountingImportDataPersister : IImportDataPersister
    {
        public int PersistCallCount { get; private set; }

        public Task<ImportPersistResult> SaveAsync(
            IReadOnlyList<IReadOnlyDictionary<string, object?>> rows,
            TemplateDefinition template,
            CancellationToken cancellationToken = default)
        {
            PersistCallCount++;
            return Task.FromResult(new ImportPersistResult { SavedCount = rows.Count });
        }
    }

    [Fact]
    public async Task ValidateOnly_SkipsPersist_StillValidatesRows()
    {
        var persister = new CountingImportDataPersister();
        var processor = new MiniExcelImportProcessor(
            new ImportRequestValidator(ProcessorTestFactory.OptionsSnapshot),
            new DefaultRowValidator(new DefaultUniqueValueChecker()),
            persister,
            ProcessorTestFactory.OptionsSnapshot,
            NullLogger<MiniExcelImportProcessor>.Instance);

        await using var file = CustomerTestData.SampleCustomersWorkbook();
        var work = ImportWorkFactory.Create(
            file,
            ImportWorkFactory.Template(),
            phase: ImportExecutionPhase.ValidateOnly);

        var result = await processor.ImportAsync(work, CancellationToken.None);

        Assert.Equal(0, persister.PersistCallCount);
        Assert.Equal(0, result.SavedCount);
        Assert.Equal(3, result.ValidRowCount);
        Assert.Equal(0, result.FailedRowCount);
    }

    [Fact]
    public async Task Import_PersistsRows()
    {
        var persister = new CountingImportDataPersister();
        var processor = new MiniExcelImportProcessor(
            new ImportRequestValidator(ProcessorTestFactory.OptionsSnapshot),
            new DefaultRowValidator(new DefaultUniqueValueChecker()),
            persister,
            ProcessorTestFactory.OptionsSnapshot,
            NullLogger<MiniExcelImportProcessor>.Instance);

        await using var file = CustomerTestData.SampleCustomersWorkbook();
        var work = ImportWorkFactory.Create(
            file,
            ImportWorkFactory.Template(),
            phase: ImportExecutionPhase.Import);

        var result = await processor.ImportAsync(work, CancellationToken.None);

        Assert.True(persister.PersistCallCount > 0);
        Assert.Equal(3, result.SavedCount);
        Assert.Equal(3, result.ValidRowCount);
    }
}
