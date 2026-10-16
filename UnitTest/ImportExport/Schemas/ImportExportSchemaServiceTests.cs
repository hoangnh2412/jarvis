using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace UnitTest.ImportExport.Schemas;

public class ImportExportSchemaServiceTests
{
    private sealed class TestImportRow
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(1, 99)]
        public int Age { get; set; }

        public Guid Id { get; set; }

        public Guid TenantId { get; set; }
    }

    private static ImportExportSchemaService CreateSut(
        Action<ImportExportEntityRegistryOptions>? configure = null)
    {
        configure ??= o => o.Add<TestImportRow>("Test", "Test Entity");
        var options = Options.Create(BuildOptions(configure));
        var registry = new ImportExportEntityRegistry(options);
        return new ImportExportSchemaService(registry, NullLogger<ImportExportSchemaService>.Instance);
    }

    private static ImportExportEntityRegistryOptions BuildOptions(
        Action<ImportExportEntityRegistryOptions> configure)
    {
        var options = new ImportExportEntityRegistryOptions();
        configure(options);
        return options;
    }

    [Fact]
    public void ListEntities_ReturnsRegisteredSummary()
    {
        var sut = CreateSut();
        var list = sut.ListEntities();

        Assert.Single(list);
        Assert.Equal("Test", list[0].Key);
        Assert.Equal("Test Entity", list[0].DisplayName);
    }

    [Fact]
    public void GetFields_ExcludesIdAndTenantId()
    {
        var sut = CreateSut();
        var fields = sut.GetFields("Test");

        Assert.DoesNotContain(fields, f => f.Name is "Id" or "TenantId");
        Assert.Equal(2, fields.Count);
    }

    [Fact]
    public void GetFields_MapsValidationMetadata()
    {
        var sut = CreateSut();
        var name = sut.GetFields("Test").Single(f => f.Name == "Name");
        var age = sut.GetFields("Test").Single(f => f.Name == "Age");

        Assert.True(name.Required);
        Assert.Equal(100, name.MaxLength);
        Assert.Equal(1, age.Min);
        Assert.Equal(99, age.Max);
        Assert.Equal("string", name.DataType);
        Assert.False(name.IsNumeric);
        Assert.Equal("int", age.DataType);
        Assert.True(age.IsNumeric);
    }

    [Fact]
    public void GetFields_UnknownKey_ThrowsNotFound()
    {
        var sut = CreateSut();
        Assert.Throws<ImportExportEntityNotFoundException>(() => sut.GetFields("Missing"));
    }
}
