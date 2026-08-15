using Jarvis.DDD.Domain.Querying;

namespace UnitTest.Querying;

public class FilterParserTests
{
    [Fact]
    public void Parse_NullOrEmpty_ReturnsNull()
    {
        Assert.Null(FilterParser.Parse(null));
        Assert.Null(FilterParser.Parse(""));
        Assert.Null(FilterParser.Parse("  "));
    }

    [Fact]
    public void Parse_ValidCondition_ReturnsConditionNode()
    {
        var json = """["Age", ">", 10]""";
        var result = FilterParser.Parse(json);
        
        Assert.NotNull(result);
        var condition = Assert.IsType<FilterCondition>(result);
        Assert.Equal("Age", condition.FieldName);
        Assert.Equal(FilterOperator.GreaterThan, condition.Operator);
        // Numbers are parsed as strings internally to preserve precision until Expression building
        Assert.Equal("10", condition.Value?.ToString());
    }

    [Fact]
    public void Parse_NotEqualAlias_ReturnsConditionNode()
    {
        var json = """["Age", "<>", 10]""";
        var result = FilterParser.Parse(json);
        
        Assert.NotNull(result);
        var condition = Assert.IsType<FilterCondition>(result);
        Assert.Equal("Age", condition.FieldName);
        Assert.Equal(FilterOperator.NotEqual, condition.Operator);
    }

    [Fact]
    public void Parse_ValidStringCondition_ReturnsConditionNode()
    {
        var json = """["Name", "contains", "John"]""";
        var result = FilterParser.Parse(json);
        
        Assert.NotNull(result);
        var condition = Assert.IsType<FilterCondition>(result);
        Assert.Equal("Name", condition.FieldName);
        Assert.Equal(FilterOperator.Contains, condition.Operator);
        Assert.Equal("John", condition.Value);
    }

    [Theory]
    [InlineData("between", FilterOperator.Between)]
    [InlineData("in", FilterOperator.In)]
    [InlineData("isnull", FilterOperator.IsNull)]
    [InlineData("isnotnull", FilterOperator.IsNotNull)]
    public void Parse_VariousOperators_ReturnsCorrectOperator(string opString, FilterOperator expectedOp)
    {
        var json = $$"""["Field", "{{opString}}", null]""";
        var result = FilterParser.Parse(json);
        var condition = Assert.IsType<FilterCondition>(result);
        Assert.Equal(expectedOp, condition.Operator);
    }

    [Fact]
    public void Parse_UnsupportedOperator_ThrowsInvalidFilterException()
    {
        var json = """["Name", "xyz", "John"]""";
        var ex = Assert.Throws<InvalidFilterException>(() => FilterParser.Parse(json));
        Assert.Contains("Unsupported operator 'xyz'", ex.Message);
    }

    [Fact]
    public void Parse_AndGroup_ReturnsGroupNode()
    {
        var json = """[["Age", ">", 10], "and", ["Name", "=", "John"]]""";
        var result = FilterParser.Parse(json);
        
        Assert.NotNull(result);
        var group = Assert.IsType<FilterGroup>(result);
        Assert.Equal(LogicOperator.And, group.Logic);
        Assert.Equal(2, group.Nodes.Count);
        
        var cond1 = Assert.IsType<FilterCondition>(group.Nodes[0]);
        Assert.Equal("Age", cond1.FieldName);
        
        var cond2 = Assert.IsType<FilterCondition>(group.Nodes[1]);
        Assert.Equal("Name", cond2.FieldName);
    }

    [Fact]
    public void Parse_OrGroup_ReturnsGroupNode()
    {
        var json = """[["Status", "=", "A"], "OR", ["Status", "=", "B"]]""";
        var result = FilterParser.Parse(json);
        
        Assert.NotNull(result);
        var group = Assert.IsType<FilterGroup>(result);
        Assert.Equal(LogicOperator.Or, group.Logic);
        Assert.Equal(2, group.Nodes.Count);
    }

    [Fact]
    public void Parse_NestedGroups_ReturnsCorrectTree()
    {
        var json = """[["A", "=", 1], "and", [["B", "=", 2], "or", ["C", "=", 3]]]""";
        var result = FilterParser.Parse(json);
        
        var rootGroup = Assert.IsType<FilterGroup>(result);
        Assert.Equal(LogicOperator.And, rootGroup.Logic);
        Assert.Equal(2, rootGroup.Nodes.Count);

        var innerGroup = Assert.IsType<FilterGroup>(rootGroup.Nodes[1]);
        Assert.Equal(LogicOperator.Or, innerGroup.Logic);
        Assert.Equal(2, innerGroup.Nodes.Count);
    }

    [Fact]
    public void Parse_EvenNumberOfElementsInGroup_ThrowsInvalidFilterException()
    {
        var json = """[["A", "=", 1], "and"]"""; // missing right side
        var ex = Assert.Throws<InvalidFilterException>(() => FilterParser.Parse(json));
        Assert.Contains("odd number of elements", ex.Message);
    }

    [Fact]
    public void Parse_MixedLogic_ThrowsInvalidFilterException()
    {
        var json = """[["A", "=", 1], "and", ["B", "=", 2], "or", ["C", "=", 3]]""";
        
        var ex = Assert.Throws<InvalidFilterException>(() => FilterParser.Parse(json));
        Assert.Contains("Cannot mix 'AND' and 'OR' at the same level", ex.Message);
    }

    [Fact]
    public void Parse_InvalidJson_ThrowsInvalidFilterException()
    {
        var json = """["Age", ">", 10"""; // Missing closing bracket
        
        Assert.Throws<InvalidFilterException>(() => FilterParser.Parse(json));
    }

    [Fact]
    public void Parse_ExceedsMaxDepth_ThrowsInvalidFilterException()
    {
        // 10 levels deep
        var json = "[[[[[[[[[[\"A\", \"=\", 1]]]]]]]]]]";
        
        var ex = Assert.Throws<InvalidFilterException>(() => FilterParser.Parse(json));
        Assert.Contains("depth exceeds maximum", ex.Message);
    }

    [Fact]
    public void Parse_ExceedsMaxConditions_ThrowsInvalidFilterException()
    {
        // Generate a JSON array with 51 conditions connected by "and"
        var conditions = new System.Collections.Generic.List<string>();
        for (int i = 0; i < 51; i++)
        {
            conditions.Add("[\"A\", \"=\", 1]");
        }
        var json = "[" + string.Join(", \"and\", ", conditions) + "]";
        
        var ex = Assert.Throws<InvalidFilterException>(() => FilterParser.Parse(json));
        Assert.Contains("exceeds maximum of 50", ex.Message);
    }
}
