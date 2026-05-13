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

    public virtual void SetTenantId(string? tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            TenantId = null;
            return;
        }

        if (!Guid.TryParse(tenantId, out var id))
            throw new ArgumentException("Tenant id must be a valid GUID.", nameof(tenantId));

        TenantId = id;
    }
}