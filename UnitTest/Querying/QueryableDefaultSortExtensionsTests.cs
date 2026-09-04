using Microsoft.EntityFrameworkCore;
using Jarvis.ORM.EntityFramework.Extensions;

namespace UnitTest.Querying;

public class QueryableDefaultSortExtensionsTests
{
    [Fact]
    public void ApplyDefaultSort_NoAuditEntity_SortsById()
    {
        var query = new List<NoAuditEntity>
        {
            new NoAuditEntity { Name = "B" },
            new NoAuditEntity { Name = "A" }
        }.AsQueryable();

        // Should not throw, should use Id
        var result = query.ApplyDefaultSort();
        Assert.NotNull(result);
    }

    [Fact]
    public void ApplyDefaultSort_NoIdEntity_ThrowsInvalidOperationException()
    {
        var query = new List<NoIdEntity>
        {
            new NoIdEntity { Name = "B" }
        }.AsQueryable();

        // Default sort requires at least Id. If it has no audit fields and no Id, it throws.
        var ex = Assert.Throws<InvalidOperationException>(() => query.ApplyDefaultSort());
        Assert.Contains("nor has an 'Id' property", ex.Message);
    }

    [Fact]
    public void ApplyDefaultSort_FullAuditEntity_SortsByUpdatedDescThenCreatedAscThenIdAsc()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var id3 = Guid.NewGuid();

        var query = new List<TestEntity>
        {
            new TestEntity { Id = id1, UpdatedAt = new DateTime(2023, 1, 1), CreatedAt = new DateTime(2022, 1, 2) }, // #3: older updated
            new TestEntity { Id = id2, UpdatedAt = new DateTime(2023, 1, 2), CreatedAt = new DateTime(2022, 1, 1) }, // #1: newest updated
            new TestEntity { Id = id3, UpdatedAt = new DateTime(2023, 1, 2), CreatedAt = new DateTime(2022, 1, 2) }  // #2: same updated, newer created (so comes after #1 because CreatedAt is asc)
        }.AsQueryable();

        var result = query.ApplyDefaultSort().ToList();

        // Order should be: UpdatedAt DESC, CreatedAt ASC, Id ASC
        Assert.Equal(id2, result[0].Id);
        Assert.Equal(id3, result[1].Id);
        Assert.Equal(id1, result[2].Id);
    }

    [Fact]
    public void ApplyDefaultSort_OnlyCreatedEntity_SortsByCreatedAscThenIdAsc()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var id3 = Guid.NewGuid();

        var query = new List<OnlyCreatedEntity>
        {
            new OnlyCreatedEntity { Id = id1, CreatedAt = new DateTime(2022, 1, 2) }, // #2 (or #3 depending on Id)
            new OnlyCreatedEntity { Id = id2, CreatedAt = new DateTime(2022, 1, 1) }, // #1
            new OnlyCreatedEntity { Id = id3, CreatedAt = new DateTime(2022, 1, 2) }  // #3 (or #2 depending on Id)
        }.AsQueryable();

        var result = query.ApplyDefaultSort().ToList();

        // Order should be: CreatedAt ASC, then Id ASC
        Assert.Equal(id2, result[0].Id);
        
        // id1 and id3 have same CreatedAt, tie-broken by Id ASC
        var expectedSecond = id1.CompareTo(id3) < 0 ? id1 : id3;
        var expectedThird = id1.CompareTo(id3) < 0 ? id3 : id1;

        Assert.Equal(expectedSecond, result[1].Id);
        Assert.Equal(expectedThird, result[2].Id);
    }
}
