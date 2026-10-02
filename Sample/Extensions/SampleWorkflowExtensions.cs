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
        var redisConfiguration = builder.Configuration["Cache:DistributedGroups:Redis:Default:Configuration"]
            ?? throw new InvalidOperationException("Cache:DistributedGroups:Redis:Default:Configuration is not configured.");

        builder.Services.AddPlatformWorkflowServices(
            redisConfiguration,
            options => options.ConfigureFromSection(builder.Configuration));

        var workflowModule = builder.AddElsaWorkflowModule();
        var workflowOptions = ElsaWorkflowOptions.FromConfiguration(builder.Configuration);

        if (workflowOptions.Mode == WorkflowExecutionMode.Embedded)
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

    public static WebApplication UseSampleWorkflow(this WebApplication app, string? loginPath = null)
    {
        var workflowOptions = ElsaWorkflowOptions.FromConfiguration(app.Configuration);

        if (workflowOptions.Mode == WorkflowExecutionMode.Embedded)
        {
            app.UseWorkflowApi();
            app.UseWorkflowStudio(loginPath);
        }
        else
        {
            var workflowStudioUrl = workflowOptions.GetResolvedStudioUrl()
                ?? throw new InvalidOperationException(
                    "Elsa Workflow Studio URL is not configured. Configure 'Elsa:StudioUrl' or 'Elsa:ServerUrl'.");

            app.MapGet("/preview/{definitionId}/{instanceId?}",
                (string definitionId, string? instanceId) =>
                {
                    var previewPath = $"/preview/{Uri.EscapeDataString(definitionId)}";
                    if (!string.IsNullOrWhiteSpace(instanceId))
                        previewPath += $"/{Uri.EscapeDataString(instanceId)}";

                    return Results.Redirect($"{workflowStudioUrl}{previewPath}");
                });
        }

        return app;
    }
}
