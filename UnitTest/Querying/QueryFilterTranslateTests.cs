using Microsoft.EntityFrameworkCore;
using Jarvis.DDD.Domain.Entities;
using Jarvis.DDD.Domain.Querying;
using Jarvis.ORM.EntityFramework.Extensions;

namespace UnitTest.Querying;

public class QueryFilterTranslateTests
{
    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        var context = new TestDbContext(options);
        
        context.TestEntities.AddRange(
            new TestEntity { FirstName = "John", LastName = "Doe", Age = 25, Amount = 100.5m, IsActive = true, NullableField = "A", CreatedAt = new DateTime(2023, 1, 1), Birthday = new DateOnly(1990, 1, 1), Status = TestStatus.Pending, CreatedBy = Guid.Parse("11111111-1111-1111-1111-111111111111"), DoubleValue = 10.5, FloatValue = 5.5f, DateTimeOffsetValue = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero) },
            new TestEntity { FirstName = "Jane", LastName = "[Smith]", Age = 30, Amount = 200.0m, IsActive = false, NullableField = null, CreatedAt = new DateTime(2023, 6, 1), Birthday = new DateOnly(1985, 6, 1), Status = TestStatus.Active, CreatedBy = Guid.Parse("22222222-2222-2222-2222-222222222222"), DoubleValue = 20.5, FloatValue = 10.5f, DateTimeOffsetValue = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.FromHours(7)) },
            new TestEntity { FirstName = "Johnny", LastName = "Depp", Age = 50, Amount = 500.75m, IsActive = true, NullableField = "B", CreatedAt = new DateTime(2023, 12, 1), Birthday = new DateOnly(1975, 12, 1), Status = TestStatus.Closed, CreatedBy = Guid.Parse("33333333-3333-3333-3333-333333333333"), DoubleValue = 30.5, FloatValue = 15.5f, DateTimeOffsetValue = new DateTimeOffset(2023, 12, 1, 0, 0, 0, TimeSpan.FromHours(-5)) }
        );
        context.SaveChanges();
        
        return context;
    }

    private readonly ISet<string> _allowedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase) 
    { 
        "FirstName", "LastName", "Age", "Amount", "IsActive", "NullableField", "CreatedAt", "CreatedBy", "Status", "Birthday", "Id", "DoubleValue", "FloatValue", "DateTimeOffsetValue"
    };

    [Fact]
    public async Task ApplyDynamicFilter_Equal_FiltersCorrectly()
    {
        await using var context = CreateContext();
        var ast = FilterParser.Parse("""["Age", "=", "25"]""");
        
        var query = context.TestEntities.ApplyDynamicFilter(ast, _allowedFields);
        var result = await query.ToListAsync();
        
        Assert.Single(result);
        Assert.Equal("John", result[0].FirstName);
    }

    [Fact]
    public async Task ApplyDynamicFilter_NotEqual_FiltersCorrectly()
    {
        await using var context = CreateContext();
        var ast = FilterParser.Parse("""["Age", "!=", "25"]""");
        
        var query = context.TestEntities.ApplyDynamicFilter(ast, _allowedFields);
        var result = await query.ToListAsync();
        
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task ApplyDynamicFilter_GreaterThan_FiltersCorrectly()
    {
        await using var context = CreateContext();
        var ast = FilterParser.Parse("""["Age", ">", "25"]""");
        
        var query = context.TestEntities.ApplyDynamicFilter(ast, _allowedFields);
        var result = await query.ToListAsync();
        
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task ApplyDynamicFilter_ContainsString_FiltersCorrectly()
    {
        await using var context = CreateContext();
        var ast = FilterParser.Parse("""["FirstName", "contains", "John"]""");
        
        var query = context.TestEntities.ApplyDynamicFilter(ast, _allowedFields);
        var result = await query.ToListAsync();
        
        Assert.Equal(2, result.Count); // John and Johnny
    }

    [Fact]
    public async Task ApplyDynamicFilter_InList_FiltersCorrectly()
    {
        await using var context = CreateContext();
        var ast = FilterParser.Parse("""["Age", "in", [25, 30]]""");
        
        var query = context.TestEntities.ApplyDynamicFilter(ast, _allowedFields);
        var result = await query.ToListAsync();
        
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task ApplyDynamicFilter_Between_FiltersCorrectly()
    {
        await using var context = CreateContext();
        var ast = FilterParser.Parse("""["Amount", "between", [100.5, 300.0]]""");
        
        var query = context.TestEntities.ApplyDynamicFilter(ast, _allowedFields);
        var result = await query.ToListAsync();
        
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task ApplyDynamicFilter_IsNull_FiltersCorrectly()
    {
        await using var context = CreateContext();
        var ast = FilterParser.Parse("""["NullableField", "isnull", null]""");
        
        var query = context.TestEntities.ApplyDynamicFilter(ast, _allowedFields);
        var result = await query.ToListAsync();
        
        Assert.Single(result);
        Assert.Equal("Jane", result[0].FirstName);
    }

    [Fact]
    public async Task ApplyDynamicFilter_AndGroup_FiltersCorrectly()
    {
        await using var context = CreateContext();
        var ast = FilterParser.Parse("""[["Age", ">", 25], "and", ["IsActive", "=", true]]""");
        
        var query = context.TestEntities.ApplyDynamicFilter(ast, _allowedFields);
        var result = await query.ToListAsync();
        
        Assert.Single(result);
        Assert.Equal("Johnny", result[0].FirstName);
    }

    [Theory]
    [InlineData("<", "30", 1)] // John (25)
    [InlineData("<=", "30", 2)] // John (25), Jane (30)
    [InlineData(">=", "30", 2)] // Jane (30), Johnny (50)
    public async Task ApplyDynamicFilter_ComparisonOperators_FiltersCorrectly(string opString, string valString, int expectedCount)
    {
        await using var context = CreateContext();
        var ast = FilterParser.Parse($$"""["Age", "{{opString}}", "{{valString}}"]""");
        
        var query = context.TestEntities.ApplyDynamicFilter(ast, _allowedFields);
        var result = await query.ToListAsync();
        
        Assert.Equal(expectedCount, result.Count);
    }

    [Theory]
    [InlineData("notcontains", "John", 1)] // Jane
    [InlineData("startswith", "Joh", 2)] // John, Johnny
    [InlineData("endswith", "e", 1)] // Doe, Jane (but field is FirstName, so Jane only)
    public async Task ApplyDynamicFilter_StringOperators_FiltersCorrectly(string opString, string valString, int expectedCount)
    {
        await using var context = CreateContext();
        var ast = FilterParser.Parse($$"""["FirstName", "{{opString}}", "{{valString}}"]""");
        
        var query = context.TestEntities.ApplyDynamicFilter(ast, _allowedFields);
        var result = await query.ToListAsync();
        
        Assert.Equal(expectedCount, result.Count);
    }

    [Fact]
    public async Task ApplyDynamicFilter_IsNotNull_FiltersCorrectly()
    {
        await using var context = CreateContext();
        var ast = FilterParser.Parse("""["NullableField", "isnotnull", null]""");
        
        var query = context.TestEntities.ApplyDynamicFilter(ast, _allowedFields);
        var result = await query.ToListAsync();
        
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task ApplyDynamicFilter_TypeConversion_DateTime()
    {
        await using var context = CreateContext();
        var ast = FilterParser.Parse("""["CreatedAt", ">=", "2023-06-01"]""");
        
        var query = context.TestEntities.ApplyDynamicFilter(ast, _allowedFields);
        var result = await query.ToListAsync();
        
        Assert.Equal(2, result.Count); // Jane, Johnny
    }

    [Fact]
    public async Task ApplyDynamicFilter_TypeConversion_DateOnly()
    {
        await using var context = CreateContext();
        var ast = FilterParser.Parse("""["Birthday", "=", "1985-06-01"]""");
        
        var query = context.TestEntities.ApplyDynamicFilter(ast, _allowedFields);
        var result = await query.ToListAsync();
        
        Assert.Single(result);
        Assert.Equal("Jane", result[0].FirstName);
    }

    [Theory]
    [InlineData("Active")]
    [InlineData("1")]
    public async Task ApplyDynamicFilter_TypeConversion_Enum(string val)
    {
        await using var context = CreateContext();
        var ast = FilterParser.Parse($$"""["Status", "=", "{{val}}"]""");
        
        var query = context.TestEntities.ApplyDynamicFilter(ast, _allowedFields);
        var result = await query.ToListAsync();
        
        Assert.Single(result);
        Assert.Equal("Jane", result[0].FirstName);
    }

    [Fact]
    public async Task ApplyDynamicFilter_TypeConversion_Guid()
    {
        await using var context = CreateContext();
        var ast = FilterParser.Parse("""["CreatedBy", "=", "22222222-2222-2222-2222-222222222222"]""");
        
        var query = context.TestEntities.ApplyDynamicFilter(ast, _allowedFields);
        var result = await query.ToListAsync();
        
        Assert.Single(result);
        Assert.Equal("Jane", result[0].FirstName);
    }

    [Fact]
    public async Task ApplyDynamicFilter_NestedGroups_FiltersCorrectly()
    {
        await using var context = CreateContext();
        // (Age > 20 OR IsActive = false) AND (Amount > 150)
        // Age > 20 OR IsActive = false => All 3 (John 25, Jane inactive, Johnny 50)
        // Amount > 150 => Jane (200), Johnny (500.75)
        // Expected: 2
        var json = """[[["Age", ">", 20], "or", ["IsActive", "=", false]], "and", ["Amount", ">", 150]]""";
        var ast = FilterParser.Parse(json);
        
        var query = context.TestEntities.ApplyDynamicFilter(ast, _allowedFields);
        var result = await query.ToListAsync();
        
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task ApplyDynamicFilter_TypeConversion_Double()
    {
        await using var context = CreateContext();
        var ast = FilterParser.Parse("""["DoubleValue", ">=", "20.5"]""");
        
        var query = context.TestEntities.ApplyDynamicFilter(ast, _allowedFields);
        var result = await query.ToListAsync();
        
        Assert.Equal(2, result.Count); // Jane, Johnny
    }

    [Fact]
    public async Task ApplyDynamicFilter_TypeConversion_Float()
    {
        await using var context = CreateContext();
        var ast = FilterParser.Parse("""["FloatValue", "<", "15.0"]""");
        
        var query = context.TestEntities.ApplyDynamicFilter(ast, _allowedFields);
        var result = await query.ToListAsync();
        
        Assert.Equal(2, result.Count); // John, Jane
    }

    [Fact]
    public async Task ApplyDynamicFilter_TypeConversion_DateTimeOffset()
    {
        await using var context = CreateContext();
        var ast = FilterParser.Parse("""["DateTimeOffsetValue", "=", "2023-06-01T00:00:00.000+07:00"]""");
        
        var query = context.TestEntities.ApplyDynamicFilter(ast, _allowedFields);
        var result = await query.ToListAsync();
        
        Assert.Single(result);
        Assert.Equal("Jane", result[0].FirstName);
    }

    [Fact]
    public async Task ApplyDynamicFilter_ContainsWithSpecialChars_EscapesCorrectly()
    {
        await using var context = CreateContext();
        // Jane's LastName is "[Smith]". We search for "[" literally.
        var ast = FilterParser.Parse("""["LastName", "contains", "["]""");
        
        var query = context.TestEntities.ApplyDynamicFilter(ast, _allowedFields);
        var result = await query.ToListAsync();
        
        Assert.Single(result);
        Assert.Equal("Jane", result[0].FirstName);
    }

    [Fact]
    public async Task ApplyDynamicFilter_Equal_NullPath()
    {
        await using var context = CreateContext();
        // Should delegate to BuildIsNull
        var ast = FilterParser.Parse("""["NullableField", "=", null]""");
        
        var query = context.TestEntities.ApplyDynamicFilter(ast, _allowedFields);
        var result = await query.ToListAsync();
        
        Assert.Single(result);
        Assert.Equal("Jane", result[0].FirstName);
    }

    [Fact]
    public void ApplyDynamicFilter_Between_WrongArrayLength_ThrowsInvalidFilterException()
    {
        using var context = CreateContext();
        var ast = FilterParser.Parse("""["Age", "between", [25]]"""); // Array of length 1
        
        var ex = Assert.Throws<InvalidFilterException>(() => context.TestEntities.ApplyDynamicFilter(ast, _allowedFields));
        Assert.Contains("requires an array of exactly 2 values", ex.Message);
    }
}
