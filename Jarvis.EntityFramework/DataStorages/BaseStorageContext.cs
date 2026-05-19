using Jarvis.Domain.Entities;
using Jarvis.Domain.Repositories;
using Jarvis.EntityFramework.Extensions;
using Jarvis.EntityFramework.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Jarvis.EntityFramework.DataStorages;

public abstract class BaseStorageContext<TDbContext>(
    DbContextOptions<TDbContext> options)
    : DbContext(options), IStorageContext
    where TDbContext : DbContext
{
    public Guid? TenantId { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        AddConcurrencyCheck(modelBuilder);
        AddGlobalQueryFilter(modelBuilder);
    }

    protected virtual bool ShouldFilterTenant(Type entityType)
    {
        if (typeof(ITenantEntity).IsAssignableFrom(entityType))
            return true;

        return false;
    }

    private void AddGlobalQueryFilter(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            if (!ShouldFilterTenant(clrType))
                continue;

            modelBuilder.Entity(clrType).AddQueryFilter<ITenantEntity>(x => x.TenantId == TenantId);
        }
    }

    private static void AddConcurrencyCheck(ModelBuilder modelBuilder)
    {
        var checkTypes = TypeHelper.GetAllClassSubTypes<IConcurrencyCheck>();
        if (checkTypes == null || !checkTypes.Any())
            return;

        foreach (var type in checkTypes)
        {
            if (type.Name.Contains("AggregateRoot") || type.Name.Contains("BaseEntity"))
                continue;

            modelBuilder.Entity(type, action =>
            {
                action.Property("RowVersion")
                    .IsRowVersion()
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate();
            });

        }
    }

    public virtual void SetTenantId(Guid? tenantId) => TenantId = tenantId;

    public override int SaveChanges()
    {
        TenantScopedContextValidation.EnsureTenantIdForSave(this);
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        TenantScopedContextValidation.EnsureTenantIdForSave(this);
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        TenantScopedContextValidation.EnsureTenantIdForSave(this);
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        TenantScopedContextValidation.EnsureTenantIdForSave(this);
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}