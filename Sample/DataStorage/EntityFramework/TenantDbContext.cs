using System.Diagnostics.CodeAnalysis;
using Jarvis.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Sample.DataStorage.EntityFramework;

public class TenantDbContext : DbContext, IStorageContext
{
    public TenantDbContext([NotNullAttribute] DbContextOptions<TenantDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("public");

        modelBuilder.Entity<Tenant>(builder =>
        {
            builder.ToTable("tenants");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).IsRequired();
        });
    }
}