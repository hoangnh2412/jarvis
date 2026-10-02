using Elsa.Extensions;
using Elsa.Features.Services;
using Elsa.Workflows;
using Microsoft.Extensions.Hosting;
using Platform.Workflow.Configuration;
using Platform.Workflow.Extensions;

namespace Platform.Modules.ElsaWorkflow.Extensions;

public static class ElsaWorkflowModuleExtensions
{
    public static ElsaWorkflowModuleBuilder AddElsaWorkflowModule(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return new ElsaWorkflowModuleBuilder(builder);
    }
}

public sealed class ElsaWorkflowModuleBuilder(IHostApplicationBuilder hostBuilder)
{
    private readonly List<Action<IModule>> _elsaConfigurators = [];

    public IHostApplicationBuilder HostBuilder { get; } = hostBuilder;

    public ElsaWorkflowModuleBuilder ConfigureElsa(Action<IModule> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        _elsaConfigurators.Add(configure);
        return this;
    }

    public ElsaWorkflowModuleBuilder AddWorkflow<TWorkflow>()
        where TWorkflow : class, IWorkflow, new()
    {
        return ConfigureElsa(elsa =>
        {
            elsa.AddActivitiesFrom<TWorkflow>();
            elsa.AddWorkflow<TWorkflow>();
        });
    }

    /// <summary>
    /// Runs the Elsa engine in the current application process.
    /// API, Studio and persistence are opt-in capabilities configured before this call.
    /// </summary>
    public IHostApplicationBuilder ApplyEmbedded()
    {
        HostBuilder.AddPlatformElsaWorkflow(ConfigureElsa);
        return HostBuilder;
    }

    /// <summary>
    /// Runs the Elsa engine in a dedicated Workflow Server application.
    /// This is the server side of the standalone deployment model.
    /// </summary>
    public IHostApplicationBuilder ApplyStandaloneServer()
    {
        return ApplyElsaHost();
    }

    /// <summary>
    /// Configures this application as a standalone workflow client. The Elsa engine,
    /// persistence, API and Studio remain in the standalone Workflow Server.
    /// </summary>
    public IHostApplicationBuilder ApplyStandaloneClient()
    {
        HostBuilder.AddPlatformWorkflowClient();
        return HostBuilder;
    }

    private IHostApplicationBuilder ApplyElsaHost()
    {
        HostBuilder.AddPlatformElsaWorkflow(ConfigureElsa);
        return HostBuilder;
    }

    private void ConfigureElsa(IModule elsa)
    {
        foreach (var configure in _elsaConfigurators)
        {
            configure(elsa);
        }
    }
}
