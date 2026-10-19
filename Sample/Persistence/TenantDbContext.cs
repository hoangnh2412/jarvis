using Platform.ORM.EntityFramework.DataStorages;
using Microsoft.EntityFrameworkCore;
using Sample.Entities;

namespace Sample.Persistence;

public class TenantDbContext(
    DbContextOptions<TenantDbContext> options)
    : BaseStorageContext(options)
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<BasicAuthUser> BasicAuthUsers => Set<BasicAuthUser>();
    public DbSet<ApiKeyCredential> ApiKeyCredentials => Set<ApiKeyCredential>();
    public DbSet<TenantPasswordPolicy> TenantPasswordPolicies => Set<TenantPasswordPolicy>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<SampleExcelTemplate> ExcelTemplates => Set<SampleExcelTemplate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new StudentEntityConfiguration());
        modelBuilder.ApplyConfiguration(new BasicAuthUserEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ApiKeyCredentialEntityConfiguration());
        modelBuilder.ApplyConfiguration(new TenantPasswordPolicyEntityConfiguration());
        modelBuilder.ApplyConfiguration(new SampleExcelTemplateEntityConfiguration());
    }
}
