using System.Data;
using Microsoft.EntityFrameworkCore;
using Platform.DDD.Domain.Repositories;

namespace UnitTest.ImportExport.Helpers;

public sealed class TestTemplateDbContext : DbContext, IStorageContext
{
    public TestTemplateDbContext(DbContextOptions<TestTemplateDbContext> options)
        : base(options)
    {
    }

    public Guid? TenantId { get; private set; }

    public DbSet<FakeExcelTemplate> Templates => Set<FakeExcelTemplate>();

    public void SetTenantId(Guid? tenantId) => TenantId = tenantId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FakeExcelTemplate>(e =>
        {
            e.ToTable("excel_templates");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            e.HasQueryFilter(x => x.IsActive && TenantId != null && x.TenantId == TenantId);
        });
    }
}

/// <summary>Minimal UoW for DbTemplateStore unit tests (EF InMemory).</summary>
public sealed class TestTemplateUnitOfWork(TestTemplateDbContext db) : IUnitOfWork
{
    public Task<IStorageContext> GetDbContextAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IStorageContext>(db);
    }

    public Task SwitchDbContextAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<TRepository> GetRepositoryAsync<TRepository>(CancellationToken cancellationToken = default)
        where TRepository : IRepository =>
        throw new NotSupportedException();

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);

    public Task<TTransaction> BeginTransactionAsync<TTransaction>(
        IsolationLevel isolation = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task CommitTransactionAsync(CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<TTransaction> UseTransactionAsync<TTransaction>(
        IDbTransaction transaction,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public void Dispose() => db.Dispose();

    public ValueTask DisposeAsync() => db.DisposeAsync();
}
