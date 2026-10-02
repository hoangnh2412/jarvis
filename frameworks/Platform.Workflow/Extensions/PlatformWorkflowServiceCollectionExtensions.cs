using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Platform.Caching.Redis;
using Platform.Workflow.Configuration;
using Platform.Workflow.Idempotency;
using Platform.Workflow.Services;

namespace Platform.Workflow.Extensions;

public static class PlatformWorkflowServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformWorkflowServices(
        this IServiceCollection services,
        string configuration,
        Action<WorkflowStoreOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(configuration);

        var options = new WorkflowStoreOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);
        services.AddSingleton<IPlatformRedisStore>(_ => new PlatformRedisStore(configuration));
        services.AddScoped<IWorkflowNodeInfoTracker, PlatformWorkflowNodeInfoTracker>();
        services.AddScoped<IWorkflowIdempotencyStore, PlatformWorkflowIdempotencyStore>();
        return services;
    }

    public static void ConfigureFromSection(this WorkflowStoreOptions options, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(configuration);
        configuration.GetSection("Workflow:Redis").Bind(options);
    }
}
