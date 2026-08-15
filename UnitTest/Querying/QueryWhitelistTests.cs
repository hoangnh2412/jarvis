using Microsoft.EntityFrameworkCore;
using Jarvis.DDD.Domain.Querying;
using Jarvis.DDD.Domain.Repositories;
using Jarvis.ORM.EntityFramework.Extensions;
using Jarvis.ORM.EntityFramework.Repositories;

namespace UnitTest.Querying;

public class QueryWhitelistTests
{
    private static IQueryable<TestEntity> CreateQuery()
    {
        return new List<TestEntity>().AsQueryable();
    }

    [Fact]
    public void ApplyDynamicFilter_FieldNotAllowed_ThrowsInvalidFilterException()
    {
        var query = CreateQuery();
        var ast = FilterParser.Parse("""["PasswordHash", "=", "xyz"]""");
        var allowed = new HashSet<string> { "FirstName", "Age" };

        var ex = Assert.Throws<InvalidFilterException>(() => { query.ApplyDynamicFilter(ast, allowed); });
        Assert.Contains("not allowed for filtering", ex.Message);
    }

    [Fact]
    public void ApplyDynamicSort_FieldNotAllowed_ThrowsArgumentException()
    {
        var query = CreateQuery();
        var sortFields = SortParser.Parse("SecretField:asc");
        var allowed = new HashSet<string> { "FirstName", "Age" };

        var ex = Assert.Throws<ArgumentException>(() => query.ApplyDynamicSort(sortFields, allowed));
        Assert.Contains("not allowed for sorting", ex.Message);
    }

    [Fact]
    public void ApplyDynamicFilter_NoAllowedFieldsConfigured_ThrowsArgumentException()
    {
        var query = CreateQuery();
        var ast = FilterParser.Parse("""["Age", "=", 10]""");
        
        var ex = Assert.Throws<ArgumentException>(() => { query.ApplyDynamicFilter(ast, null); });
        Assert.Contains("Dynamic filtering is blocked", ex.Message);
    }

    [Fact]
    public void ApplyDynamicFilter_EmptyAllowedFieldsConfigured_ThrowsArgumentException()
    {
        var query = CreateQuery();
        var ast = FilterParser.Parse("""["Age", "=", 10]""");
        
        var ex = Assert.Throws<ArgumentException>(() => { query.ApplyDynamicFilter(ast, new HashSet<string>()); });
        Assert.Contains("Dynamic filtering is blocked", ex.Message);
    }

    [Fact]
    public void ApplyDynamicSort_NoAllowedFieldsConfigured_ThrowsArgumentException()
    {
        var query = CreateQuery();
        var sortFields = SortParser.Parse("Age:asc");
        
        var ex = Assert.Throws<ArgumentException>(() => query.ApplyDynamicSort(sortFields, null));
        Assert.Contains("Dynamic sorting is blocked", ex.Message);
    }

    [Fact]
    public void ApplyColumnSelection_FieldNotAllowed_ThrowsArgumentException()
    {
        var query = CreateQuery();
        var allowed = new HashSet<string> { "FirstName" };

        var ex = Assert.Throws<ArgumentException>(() => query.ApplyColumnSelection("FirstName, Age", allowed));
        Assert.Contains("not allowed in projection", ex.Message);
    }

    [Fact]
    public void ApplyColumnSelection_NoAllowedFieldsConfigured_ThrowsArgumentException()
    {
        var query = CreateQuery();
        var ex = Assert.Throws<ArgumentException>(() =>
            query.ApplyColumnSelection("FirstName", null));
        Assert.Contains("Dynamic column selection is blocked", ex.Message);
    }

    [Fact]
    public void ApplyColumnSelection_IdNotInWhitelist_DoesNotThrow()
    {
        var query = CreateQuery();
        // Chỉ FirstName — không có Id
        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "FirstName" };
        var exception = Record.Exception(() =>
            query.ApplyColumnSelection("FirstName", allowed));
        Assert.Null(exception); // không throw
    }

    [Fact]
    public void ApplyDynamicFilter_FieldInBlacklist_ThrowsInvalidFilterException()
    {
        var query = CreateQuery();
        var ast = FilterParser.Parse("""["FirstName", "=", "John"]""");
        var allowed = new HashSet<string> { "FirstName", "Age" };
        var denied = new HashSet<string> { "FirstName" };

        var ex = Assert.Throws<InvalidFilterException>(() => { query.ApplyDynamicFilter(ast, allowed, denied); });
        Assert.Contains("blocked by blacklist", ex.Message);
    }

    [Fact]
    public void ApplyDynamicSort_FieldInBlacklist_ThrowsArgumentException()
    {
        var query = CreateQuery();
        var sortFields = SortParser.Parse("Age:asc");
        var allowed = new HashSet<string> { "FirstName", "Age" };
        var denied = new HashSet<string> { "Age" };

        var ex = Assert.Throws<ArgumentException>(() => query.ApplyDynamicSort(sortFields, allowed, denied));
        Assert.Contains("blocked by blacklist", ex.Message);
    }

    [Fact]
    public void ApplyColumnSelection_FieldInBlacklist_ThrowsArgumentException()
    {
        var query = CreateQuery();
        var allowed = new HashSet<string> { "FirstName", "Age" };
        var denied = new HashSet<string> { "FirstName" };

        var ex = Assert.Throws<ArgumentException>(() => query.ApplyColumnSelection("FirstName", allowed, denied));
        Assert.Contains("blocked by blacklist", ex.Message);
    }
}
