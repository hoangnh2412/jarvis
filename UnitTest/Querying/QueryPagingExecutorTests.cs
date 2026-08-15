using Microsoft.EntityFrameworkCore;
using Jarvis.DDD.Domain.Repositories;
using Jarvis.ORM.EntityFramework.Repositories;

namespace UnitTest.Querying;

public class QueryPagingExecutorTests
{
    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        var context = new TestDbContext(options);
        
        for (int i = 1; i <= 25; i++)
        {
            context.TestEntities.Add(new TestEntity 
            { 
                FirstName = $"User{i}", 
                LastName = "Test", 
                Age = 20 + i,
                UpdatedAt = DateTime.UtcNow.AddDays(-i),
                CreatedAt = DateTime.UtcNow.AddDays(-i)
            });
        }
        
        context.SaveChanges();
        return context;
    }

    private readonly ISet<string> _allowedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase) 
    { 
        "FirstName", "LastName", "Age", "UpdatedAt", "CreatedAt", "Id" 
    };

    [Fact]
    public async Task ExecuteAsync_BasicPaging_ReturnsCorrectPageAndTotal()
    {
        await using var context = CreateContext();
        var request = new PagedListRequest { Page = 2, Size = 10 };
        var options = new PagedQueryOptions<TestEntity> { AllowedFields = _allowedFields };

        var (items, total) = await PagedListExecutor.ExecuteAsync(context.TestEntities, request, options);

        Assert.Equal(25, total);
        Assert.Equal(10, items.Count);
        // Default sort is UpdatedAt desc, so User1 (newest) is first.
        // Page 2 should skip 10, so it starts at User11
        Assert.Equal("User11", items[0].FirstName);
    }

    [Fact]
    public async Task ExecuteAsync_WithFilter_ReturnsFilteredTotal()
    {
        await using var context = CreateContext();
        var request = new PagedListRequest { Page = 1, Size = 10, Filter = """["Age", ">", 40]""" }; // Users 21 to 25
        var options = new PagedQueryOptions<TestEntity> { AllowedFields = _allowedFields };

        var (items, total) = await PagedListExecutor.ExecuteAsync(context.TestEntities, request, options);

        Assert.Equal(5, total);
        Assert.Equal(5, items.Count);
    }

    [Fact]
    public async Task ExecuteAsync_WithSort_OverridesDefaultSort()
    {
        await using var context = CreateContext();
        var request = new PagedListRequest { Page = 1, Size = 5, Sort = "Age:asc" };
        var options = new PagedQueryOptions<TestEntity> { AllowedFields = _allowedFields };

        var (items, total) = await PagedListExecutor.ExecuteAsync(context.TestEntities, request, options);

        Assert.Equal(25, total);
        Assert.Equal(5, items.Count);
        // Age asc should start from User1 (Age 21)
        Assert.Equal("User1", items[0].FirstName);
        Assert.Equal("User2", items[1].FirstName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ExecuteAsync_InvalidPage_ThrowsArgumentOutOfRangeException(int invalidPage)
    {
        await using var context = CreateContext();
        var request = new PagedListRequest { Page = invalidPage, Size = 10 };
        
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => PagedListExecutor.ExecuteAsync(context.TestEntities, request, (PagedQueryOptions<TestEntity>?)null));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ExecuteAsync_InvalidSize_ThrowsArgumentOutOfRangeException(int invalidSize)
    {
        await using var context = CreateContext();
        var request = new PagedListRequest { Page = 1, Size = invalidSize };
        
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => PagedListExecutor.ExecuteAsync(context.TestEntities, request, (PagedQueryOptions<TestEntity>?)null));
    }

    [Fact]
    public async Task ExecuteAsync_LargeSize_IsAllowedAndReturnsItems()
    {
        await using var context = CreateContext();
        var request = new PagedListRequest { Page = 1, Size = 1001 };
        var options = new PagedQueryOptions<TestEntity> { AllowedFields = _allowedFields };
        
        var (items, total) = await PagedListExecutor.ExecuteAsync(context.TestEntities, request, options);

        Assert.Equal(25, total);
        Assert.Equal(25, items.Count);
    }

    [Fact]
    public async Task ExecuteAsync_Determinism_NoOverlappingRecordsBetweenPages()
    {
        await using var context = CreateContext();
        // Give all entities the exact same UpdatedAt/CreatedAt to force tie-breaker on Id
        var now = DateTime.UtcNow;
        foreach(var e in context.TestEntities)
        {
            e.UpdatedAt = now;
            e.CreatedAt = now;
        }
        await context.SaveChangesAsync();

        var request1 = new PagedListRequest { Page = 1, Size = 10 };
        var request2 = new PagedListRequest { Page = 2, Size = 10 };
        var options = new PagedQueryOptions<TestEntity> { AllowedFields = _allowedFields };

        var (items1, _) = await PagedListExecutor.ExecuteAsync(context.TestEntities, request1, options);
        var (items2, _) = await PagedListExecutor.ExecuteAsync(context.TestEntities, request2, options);

        // Ensure no overlapping IDs between page 1 and page 2
        var idsPage1 = items1.Select(i => i.Id).ToHashSet();
        var idsPage2 = items2.Select(i => i.Id).ToHashSet();
        
        Assert.Empty(idsPage1.Intersect(idsPage2));
        Assert.Equal(10, idsPage1.Count);
        Assert.Equal(10, idsPage2.Count);
    }

    [Fact]
    public async Task ExecuteAsync_WithColumns_ReturnsOnlyRequestedColumns()
    {
        await using var context = CreateContext();
        var request = new PagedListRequest { Page = 1, Size = 5, Columns = "FirstName, Age" };
        var options = new PagedQueryOptions<TestEntity> { AllowedFields = _allowedFields };

        var (items, _) = await PagedListExecutor.ExecuteAsync(context.TestEntities, request, options);

        Assert.NotEmpty(items);
        var item = items[0];
        
        // Requested columns are populated
        Assert.NotNull(item.FirstName);
        Assert.NotEqual(0, item.Age);
        
        // Unrequested columns are default (null/0/false)
        Assert.Null(item.LastName);
        Assert.Equal(0, item.Amount);
        Assert.False(item.IsActive);
        
        // Id is implicitly included
        Assert.NotEqual(Guid.Empty, item.Id);
    }
}
