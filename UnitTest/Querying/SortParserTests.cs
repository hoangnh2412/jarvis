using Jarvis.DDD.Domain.Querying;

namespace UnitTest.Querying;

public class SortParserTests
{
    [Fact]
    public void Parse_NullOrEmpty_ReturnsEmptyList()
    {
        Assert.Empty(SortParser.Parse(null));
        Assert.Empty(SortParser.Parse(""));
        Assert.Empty(SortParser.Parse("  "));
    }

    [Fact]
    public void Parse_SingleFieldDefaultAsc_ReturnsList()
    {
        var result = SortParser.Parse("FirstName");
        
        Assert.Single(result);
        Assert.Equal("FirstName", result[0].FieldName);
        Assert.Equal(SortDirection.Asc, result[0].Direction);
    }

    [Fact]
    public void Parse_SingleFieldDesc_ReturnsList()
    {
        var result = SortParser.Parse("Age:desc");
        
        Assert.Single(result);
        Assert.Equal("Age", result[0].FieldName);
        Assert.Equal(SortDirection.Desc, result[0].Direction);
    }

    [Fact]
    public void Parse_MultipleFields_ReturnsList()
    {
        var result = SortParser.Parse("FirstName:asc, LastName:desc, Age");
        
        Assert.Equal(3, result.Count);
        
        Assert.Equal("FirstName", result[0].FieldName);
        Assert.Equal(SortDirection.Asc, result[0].Direction);
        
        Assert.Equal("LastName", result[1].FieldName);
        Assert.Equal(SortDirection.Desc, result[1].Direction);
        
        Assert.Equal("Age", result[2].FieldName);
        Assert.Equal(SortDirection.Asc, result[2].Direction);
    }

    [Fact]
    public void Parse_InvalidDirection_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => SortParser.Parse("FirstName:xyz"));
    }
}
