using Microsoft.EntityFrameworkCore;
using Jarvis.DDD.Domain.Repositories;
using Jarvis.ORM.EntityFramework.Repositories;

namespace UnitTest.Querying;

public class QueryCustomizeTests
{
    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        var context = new TestDbContext(options);
        
        context.TestEntities.AddRange(
            new TestEntity { FirstName = "Alice", LastName = "A", Age = 20, Amount = 100, IsActive = true, Status = TestStatus.Active },
            new TestEntity { FirstName = "Bob", LastName = "B", Age = 30, Amount = 200, IsActive = true, Status = TestStatus.Closed },
            new TestEntity { FirstName = "Charlie", LastName = "C", Age = 40, Amount = 300, IsActive = false, Status = TestStatus.Pending }
        );
        context.SaveChanges();
        return context;
    }

    private readonly ISet<string> _allowedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase) 
    { 
        "FirstName", "LastName", "Age", "Amount", "IsActive", "Status", "Id" 
    };

    [Fact]
    public async Task ExecuteAsync_WithStaticScopeOnQueryable_FiltersCorrectlyAndCombinesWithDynamic()
    {
        await using var context = CreateContext();
        var request = new PagedListRequest { Page = 1, Size = 10, Filter = """["Age", ">", 10]""" }; // All > 10
        var options = new PagedQueryOptions<TestEntity> 
        { 
            AllowedFields = _allowedFields
        };

        // Static scope on IQueryable (not PagedQueryOptions)
        var query = context.TestEntities.Where(e => e.IsActive);

        var (items, total) = await PagedListExecutor.ExecuteAsync(query, request, options);

        Assert.Equal(2, total); // Alice and Bob
        Assert.All(items, i => Assert.True(i.IsActive));
    }

    [Fact]
    public async Task ExecuteAsync_WithCustomFilter_OverridesDynamicFilter()
    {
        await using var context = CreateContext();
        var request = new PagedListRequest { Page = 1, Size = 10, Filter = """["Age", "=", 20]""" }; // Should be ignored
        var options = new PagedQueryOptions<TestEntity> 
        { 
            AllowedFields = _allowedFields,
            CustomFilter = q => q.Where(e => e.FirstName == "Charlie") // Overrides completely
        };

        var (items, total) = await PagedListExecutor.ExecuteAsync(context.TestEntities, request, options);

        Assert.Equal(1, total);
        Assert.Equal("Charlie", items[0].FirstName);
    }

    [Fact]
    public async Task ExecuteAsync_WithCustomSort_OverridesDynamicSort()
    {
        await using var context = CreateContext();
        var request = new PagedListRequest { Page = 1, Size = 10, Sort = "Age:asc" }; // Should be ignored
        var options = new PagedQueryOptions<TestEntity> 
        { 
            AllowedFields = _allowedFields,
            CustomSort = q => q.OrderByDescending(e => e.FirstName) // Overrides completely
        };

        var (items, total) = await PagedListExecutor.ExecuteAsync(context.TestEntities, request, options);

        Assert.Equal(3, total);
        Assert.Equal("Charlie", items[0].FirstName); // Charlie > Bob > Alice
    }

    [Fact]
    public async Task ExecuteAsync_FullPipeline_OrderIsCorrect()
    {
        await using var context = CreateContext();
        // Pipeline: IQueryable scope -> Filter -> Count -> Sort -> Columns -> Skip/Take
        var request = new PagedListRequest 
        { 
            Page = 1, 
            Size = 1, 
            Filter = """["Age", ">", 10]""", 
            Sort = "Age:desc",
            Columns = "FirstName"
        };
        
        var options = new PagedQueryOptions<TestEntity> 
        { 
            AllowedFields = _allowedFields
        };

        // Scope: IsActive (Alice, Bob) on IQueryable
        // Filter: Age > 10 (Alice, Bob)
        // Total Count: 2
        // Sort: Age:desc -> Bob (30), then Alice (20)
        // Columns: FirstName only
        // Skip/Take: 1 item -> Bob
        var query = context.TestEntities.Where(e => e.IsActive);

        var (items, total) = await PagedListExecutor.ExecuteAsync(query, request, options);

        Assert.Equal(2, total);
        Assert.Single(items);
        Assert.Equal("Bob", items[0].FirstName);
        // Ensure other columns are not projected
        Assert.Equal(0, items[0].Age); 
        Assert.Null(items[0].LastName);
    }
}
