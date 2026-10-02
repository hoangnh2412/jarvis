using Microsoft.EntityFrameworkCore;
using Platform.Authorization.EntityFramework.Persistence;

namespace Sample.AuthorizationDemo;

/// <summary>
/// Store phân quyền của Sample (bảng AspNet* + Policies/PolicyRules) trên database riêng
/// <c>SampleAuthorization</c>, tạo bằng <c>EnsureCreated</c> — chỉ phục vụ demo.
/// Kèm bảng <c>SampleExpenses</c> để demo Data Scope (lọc danh sách bằng SQL).
/// </summary>
public sealed class SampleAuthorizationDbContext(DbContextOptions<SampleAuthorizationDbContext> options)
    : IdentityDbContextBase(options)
{
    public DbSet<SampleExpenseRow> Expenses => Set<SampleExpenseRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SampleExpenseRow>(entity =>
        {
            entity.ToTable("SampleExpenses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(32);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
        });
    }
}
