using Jarvis.EntityFramework.DataStorages;
using Microsoft.EntityFrameworkCore;
using Sample.Entities;

namespace Sample.Persistence;

public class MasterDbContext(
    DbContextOptions<MasterDbContext> options)
    : BaseStorageContext<MasterDbContext>(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<BasicAuthUser> BasicAuthUsers => Set<BasicAuthUser>();

    public DbSet<ApiKeyCredential> ApiKeyCredentials => Set<ApiKeyCredential>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new TenantEntityConfiguration());
        modelBuilder.ApplyConfiguration(new BasicAuthUserEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ApiKeyCredentialEntityConfiguration());
    }
}
