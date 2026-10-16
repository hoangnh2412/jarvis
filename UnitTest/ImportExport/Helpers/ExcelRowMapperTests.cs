using Platform.ImportExport.MiniExcel.Helpers;
using Platform.Modules.ImportExport.Domain.Models;

namespace UnitTest.ImportExport.Helpers;

/// <summary>
/// Unit test cho <see cref="ExcelRowMapper"/> — map header Excel/CSV → FieldName.
/// </summary>
public class ExcelRowMapperTests
{
    /// <summary>
    /// Header có khoảng trắng đầu/cuối vẫn khớp ColumnMapping (trim khi so sánh).
    /// Setup: file header " name", "age "; mapping ColumnName = name, age (Required).
    /// Kỳ vọng: EnsureMappedColumnsExist không ném; MapRow lấy đúng giá trị.
    /// </summary>
    [Fact]
    public void BuildColumnKeyMap_PaddedHeaders_MatchesAndMapsValues()
    {
        var source = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            [" name"] = "Alice",
            ["age "] = 20
        };
        var mappings = new[]
        {
            new ColumnMapping("name", "Name", Required: true),
            new ColumnMapping("age", "Age", Required: true)
        };

        var columnKeyMap = ExcelRowMapper.BuildColumnKeyMap(source, mappings);
        ExcelRowMapper.EnsureMappedColumnsExist(mappings, columnKeyMap);
        var mapped = ExcelRowMapper.MapRow(source, mappings, columnKeyMap);

        Assert.Equal("Alice", mapped["Name"]);
        Assert.Equal(20, mapped["Age"]);
    }

    /// <summary>
    /// Mapping ColumnName có khoảng trắng vẫn khớp header sạch.
    /// </summary>
    [Fact]
    public void BuildColumnKeyMap_PaddedMappingColumnName_MatchesHeader()
    {
        var source = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["name"] = "Bob"
        };
        var mappings = new[] { new ColumnMapping(" name ", "Name", Required: true) };

        var columnKeyMap = ExcelRowMapper.BuildColumnKeyMap(source, mappings);
        ExcelRowMapper.EnsureMappedColumnsExist(mappings, columnKeyMap);

        Assert.True(columnKeyMap.ContainsKey(" name "));
    }
}
