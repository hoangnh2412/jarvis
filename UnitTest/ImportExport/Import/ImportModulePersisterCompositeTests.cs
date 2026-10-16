using Microsoft.Extensions.DependencyInjection;
using Platform.Modules.ImportExport.DependencyInjection;

namespace UnitTest.ImportExport.Import;

public class ImportModulePersisterCompositeTests
{
    private const string StudentKey = "Student";

    private sealed class StubStudentPersister : IImportModulePersister
    {
        public string EntityKey => StudentKey;

        public Task<ImportPersistResult> SaveAsync(
            IReadOnlyList<IReadOnlyDictionary<string, object?>> rows,
            TemplateDefinition template,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new ImportPersistResult { SavedCount = rows.Count });
    }

    private static IImportDataPersister CreateComposite(Action<IServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddImportModulePersister<StubStudentPersister>(StudentKey);
        configure?.Invoke(services);
        return services.BuildServiceProvider().GetRequiredService<IImportDataPersister>();
    }

    private static TemplateDefinition Template(string? entityKey) =>
        new()
        {
            Code = "TEST",
            EntityKey = entityKey,
            Name = "Test",
            FileName = "test.xlsx"
        };

    [Fact]
    public async Task SaveAsync_EntityKeyNull_ReturnsErrorRow0()
    {
        var composite = CreateComposite();

        var result = await composite.SaveAsync([], Template(entityKey: null), CancellationToken.None);

        Assert.Equal(0, result.SavedCount);
        Assert.Single(result.Errors);
        Assert.Equal(0, result.Errors[0].RowNumber);
        Assert.Contains("EntityKey is required", result.Errors[0].Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SaveAsync_EntityKeyStudent_DelegatesToKeyedPersister()
    {
        var composite = CreateComposite();

        var rows = new List<IReadOnlyDictionary<string, object?>>
        {
            new Dictionary<string, object?> { ["Name"] = "Alice" }
        };

        var result = await composite.SaveAsync(rows, Template(StudentKey), CancellationToken.None);

        Assert.Equal(1, result.SavedCount);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task SaveAsync_EntityKeyUnknown_ReturnsErrorRow0()
    {
        var composite = CreateComposite();

        var result = await composite.SaveAsync([], Template("Unknown"), CancellationToken.None);

        Assert.Equal(0, result.SavedCount);
        Assert.Single(result.Errors);
        Assert.Equal(0, result.Errors[0].RowNumber);
        Assert.Contains("Unknown", result.Errors[0].Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SaveAsync_EntityKeyCaseInsensitive_MatchesRegisteredPersister()
    {
        var composite = CreateComposite();

        var result = await composite.SaveAsync(
            [new Dictionary<string, object?> { ["Name"] = "Bob" }],
            Template("student"),
            CancellationToken.None);

        Assert.Equal(1, result.SavedCount);
    }

    [Fact]
    public void AddImportModulePersister_RegistersCompositeAsImportDataPersister()
    {
        var services = new ServiceCollection();
        services.AddImportModulePersister<StubStudentPersister>(StudentKey);

        Assert.Contains(
            services,
            d => d.ServiceType == typeof(IImportDataPersister)
                 && d.ImplementationType == typeof(ImportModulePersisterComposite));
    }
}
