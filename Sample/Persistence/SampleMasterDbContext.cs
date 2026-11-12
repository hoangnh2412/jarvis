using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Platform.Authorization.EntityFramework.Persistence;
using Platform.Modules.Setting.EntityFramework.Entities;
using Platform.Modules.Setting.EntityFramework.Extensions;
using Sample.Entities;

namespace Sample.Persistence;

public class SampleMasterDbContext(
    DbContextOptions<SampleMasterDbContext> options)
    : IdentityDbContextBase(options)
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

    /// <summary>
    /// Role ADMIN trên Master (identity platform) nhận Permission <c>*</c> nếu chưa có wildcard.
    /// Gọi sau khi catalog authorization đã seed role ADMIN trên cùng context.
    /// </summary>
    public async Task EnsureAdminPermissionWildcardAsync(CancellationToken cancellationToken = default)
    {
        var adminId = await Roles
            .Where(r => r.NormalizedName == "ADMIN")
            .Select(r => (Guid?)r.Id)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        if (adminId is null)
            return;

        var hasWildcard = await RoleClaims.AnyAsync(
            c => c.RoleId == adminId
                 && c.ClaimType == "Permission"
                 && (c.ClaimValue == "*" || c.ClaimValue == "System.All"),
            cancellationToken).ConfigureAwait(false);
        if (hasWildcard)
            return;

        RoleClaims.Add(new IdentityRoleClaim<Guid>
        {
            RoleId = adminId.Value,
            ClaimType = "Permission",
            ClaimValue = "*",
        });
        await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
