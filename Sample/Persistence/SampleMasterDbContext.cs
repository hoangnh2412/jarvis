using Platform.ORM.EntityFramework.DataStorages;
using Microsoft.EntityFrameworkCore;
using Platform.Modules.Setting.EntityFramework.Entities;
using Platform.Modules.Setting.EntityFramework.Extensions;
using Sample.Entities;

namespace Sample.Persistence;

public class SampleMasterDbContext(
    DbContextOptions<SampleMasterDbContext> options)
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
        modelBuilder.Entity<Tenant>().ToTable("tenant");
        modelBuilder.ConfigureSetting(options => options.TableName = "setting");
        modelBuilder.Entity<AppUser>().ToTable("app-users");
        modelBuilder.Entity<OnboardingStep>().ToTable("onboarding-steps");
        modelBuilder.Entity<BusinessWorkflowMapping>().ToTable("business-workflow-mappings");
    }
}
