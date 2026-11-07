using Elsa.Persistence.EFCore.Modules.Management;
using Elsa.Persistence.EFCore.Modules.Runtime;
using Microsoft.EntityFrameworkCore;
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
        app.EnsureMigrateDb<IMasterUnitOfWork>();
        app.EnsureMigrateTemplateDb<TenantDbContext>((config, options) => options.UsePlatformHostPostgreSql(config.GetConnectionString(SampleDbContextExtensions.TenantConnectionStringName)));

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

        await app.SeedSampleAuthorizationAsync();
        await SampleLoginScopeSeed.EnsureAsync(app);
    }
}
