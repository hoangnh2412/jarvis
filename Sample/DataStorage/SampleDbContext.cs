using System.Diagnostics.CodeAnalysis;
using Jarvis.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Sample.DataStorage;

public class SampleDbContext : DbContext, IStorageContext
{
    public SampleDbContext([NotNullAttribute] DbContextOptions<SampleDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("public");

        modelBuilder.Entity<User>(builder =>
        {
            builder.ToTable("users");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).IsRequired();
        });
    }
}