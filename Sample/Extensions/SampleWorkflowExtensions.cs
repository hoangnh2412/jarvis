using Platform.Modules.ElsaWorkflow.Api.Extensions;
using Platform.Modules.ElsaWorkflow.EntityFramework.Extensions;
using Platform.Modules.ElsaWorkflow.Extensions;
using Platform.Workflow.Configuration;
using Platform.Workflow.Extensions;
using Sample.Workflows;

namespace Sample.Extensions;

public static class SampleWorkflowExtensions
{
    public static WebApplicationBuilder AddSampleWorkflow(this WebApplicationBuilder builder)
    {
        // Bật/tắt bằng Elsa:Enabled (mặc định true).
        // Embedded: Sample chạy engine + persistence + API + Studio.
        // Standalone: Sample là client, gọi Workflow Server tại Elsa:BaseUrl.
        var enabled = builder.Configuration.GetValue("Elsa:Enabled", true);
        var options = ElsaWorkflowOptions.FromConfiguration(builder.Configuration);
        if (!enabled)
            return builder;

        var redisConfiguration = builder.Configuration["Cache:DistributedGroups:Redis:Default:Configuration"]
            ?? throw new InvalidOperationException("Cache:DistributedGroups:Redis:Default:Configuration is not configured.");

        builder.Services.AddPlatformWorkflowServices(
            redisConfiguration,
            workflowOptions => workflowOptions.ConfigureFromSection(builder.Configuration));

        var workflowModule = builder.AddElsaWorkflowModule();
        if (options.Mode == WorkflowExecutionMode.Embedded)
        {
            workflowModule
                .UseEntityFramework()
                .UseHttpApi()
                .UseStudio()
                .AddWorkflow<OnboardingProcessWorkflow>()
                .AddWorkflow<HrHandoverDeviceWorkflow>()
                .AddWorkflow<ItCreateAdWorkflow>()
                .AddWorkflow<ItCreateJiraWorkflow>()
                .AddWorkflow<ItCreateConfluenceWorkflow>()
                .AddWorkflow<ItCreateBitbucketWorkflow>()
                .AddWorkflow<HrSignProbationContractWorkflow>()
                .AddWorkflow<EmployeeConfirmOnboardWorkflow>()
                .ApplyEmbedded();
        }
        else
        {
            workflowModule.ApplyStandaloneClient();
        }

        return builder;
    }

    public static WebApplication UseWorkflowEndpoints(this WebApplication app)
    {
        var enabled = app.Configuration.GetValue("Elsa:Enabled", true);
        if (!enabled)
            return app;

        var options = ElsaWorkflowOptions.FromConfiguration(app.Configuration);
        if (options.Mode == WorkflowExecutionMode.Embedded)
        {
            app.UseWorkflowApi();
            app.UseWorkflowStudio();
            return app;
        }

        var workflowStudioUrl = options.GetResolvedStudioUrl()
            ?? throw new InvalidOperationException(
                "Elsa Workflow Studio URL is not configured. Configure 'Elsa:StudioUrl' or 'Elsa:BaseUrl'.");

        app.MapGet("/preview/{definitionId}/{instanceId?}",
            (string definitionId, string? instanceId) =>
            {
                var previewPath = $"/preview/{Uri.EscapeDataString(definitionId)}";
                if (!string.IsNullOrWhiteSpace(instanceId))
                    previewPath += $"/{Uri.EscapeDataString(instanceId)}";

                return Results.Redirect($"{workflowStudioUrl}{previewPath}");
            });

        return app;
    }
}
