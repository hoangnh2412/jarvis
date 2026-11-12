using Elsa.Persistence.EFCore.Modules.Management;
using Elsa.Persistence.EFCore.Modules.Runtime;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Platform.Authorization.EntityFramework.Persistence;
using Platform.Authorization.Services;
using Platform.Host.Postgresql.Extensions;
using Platform.ORM.EntityFramework;
using Platform.Workflow.Configuration;
using Sample.Authentication;
using Sample.AuthorizationDemo;
using Sample.Persistence;

namespace Sample.Extensions;

public static class SampleMigrateExtensions
{
    public static async Task MigrateAndSeedAsync(this WebApplication app)
    {
        app.EnsureMigrateDb<ISampleMasterUnitOfWork>();
        app.EnsureMigrateTemplateDb<SampleTenantDbContext>((config, options) => options.UsePlatformHostPostgreSql(config.GetConnectionString(SampleDbContextExtensions.TenantConnectionStringName)));

        var workflowEnabled = app.Configuration.GetValue("Elsa:Enabled", true);
        var workflowOptions = ElsaWorkflowOptions.FromConfiguration(app.Configuration);
        if (workflowEnabled && workflowOptions.Mode == WorkflowExecutionMode.Embedded)
        {
            var elsaConnectionString = app.Configuration.GetConnectionString(workflowOptions.ConnectionStringName);
            if (string.IsNullOrWhiteSpace(elsaConnectionString))
            {
                throw new InvalidOperationException(
                    $"Connection string '{workflowOptions.ConnectionStringName}' is not configured or empty.");
            }

            app.EnsureMigrateTemplateDb<ManagementElsaDbContext>((_, options) => options.UseNpgsql(elsaConnectionString));
            app.EnsureMigrateTemplateDb<RuntimeElsaDbContext>((_, options) => options.UseNpgsql(elsaConnectionString));
        }

        await app.SeedSampleMasterAuthorizationAsync();
        await app.SeedSampleAuthorizationAsync();
        // await SampleLoginScopeSeed.EnsureAsync(app);
    }

    /// <summary>Catalog platform + wildcard ADMIN trên database Master (identity đăng nhập platform).</summary>
    private static async Task SeedSampleMasterAuthorizationAsync(this WebApplication app, CancellationToken cancellationToken = default)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        try
        {
            var master = services.GetRequiredService<SampleMasterDbContext>();
            var validator = services.GetRequiredService<OperandValidator>();
            var catalog = services.GetRequiredService<AuthorizationDataSeeder>().BuildCatalog();
            await master.SeedAsync(catalog, validator, cancellationToken).ConfigureAwait(false);
            await master.EnsureAdminPermissionWildcardAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            app.Logger.LogWarning(ex, "Bỏ qua seed phân quyền Master: không chuẩn bị được database Master.");
        }
    }
}
