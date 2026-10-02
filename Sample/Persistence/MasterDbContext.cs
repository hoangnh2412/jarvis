using Platform.ORM.EntityFramework.DataStorages;
using Microsoft.EntityFrameworkCore;
using Platform.Modules.Setting.EntityFramework.Entities;
using Platform.Modules.Setting.EntityFramework.Extensions;
using Sample.Entities;

namespace Sample.Persistence;

public class MasterDbContext(
    DbContextOptions<MasterDbContext> options)
    : BaseStorageContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<Setting> Settings => Set<Setting>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();

    public DbSet<OnboardingStep> OnboardingSteps => Set<OnboardingStep>();

    public DbSet<BusinessWorkflowMapping> BusinessWorkflowMappings => Set<BusinessWorkflowMapping>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new TenantEntityConfiguration());
        modelBuilder.ConfigureSetting();
    }
}
