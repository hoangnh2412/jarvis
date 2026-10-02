using Elsa;
using Elsa.Common.Multitenancy;
using Elsa.Extensions;
using Elsa.Features.Services;
using Elsa.Persistence.EFCore.Extensions;
using Elsa.Persistence.EFCore.Modules.Management;
using Elsa.Persistence.EFCore.Modules.Runtime;
using Elsa.Tenants.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Platform.Workflow.Multitenancy;
using Microsoft.Extensions.Hosting;
using Platform.Workflow.Abstractions;
using Platform.Workflow.Configuration;
using Platform.Workflow.Services;
using Platform.DDD.Domain.Services;
using Platform.Multitenancy;
using Platform.DDD.Domain.DataStorages;
using Platform.Caching.Redis;
using Platform.Workflow.Idempotency;

namespace Platform.Workflow.Extensions;

public static class ElsaWorkflowHostBuilderExtensions
{
    public static IHostApplicationBuilder AddPlatformWorkflow(this IHostApplicationBuilder builder, Action<IModule>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var workflowOptions = ElsaWorkflowOptions.FromConfiguration(builder.Configuration);

        if (workflowOptions.Mode == WorkflowExecutionMode.Embedded)
        {
            return builder.AddPlatformElsaWorkflow(configure);
        }

        return builder.AddPlatformWorkflowClient();
    }

    public static IHostApplicationBuilder AddPlatformWorkflowClient(this IHostApplicationBuilder builder, Action<ElsaWorkflowOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var workflowOptions = ElsaWorkflowOptions.FromConfiguration(builder.Configuration);
        configureOptions?.Invoke(workflowOptions);

        builder.Services.AddSingleton(workflowOptions);

        var resolvedApiUrl = workflowOptions.GetResolvedApiUrl()
                             ?? throw new InvalidOperationException(
                                 "Elsa Workflow Server URL is not configured. " +
                                 "In Standalone mode, you must configure 'Elsa:ServerUrl' (e.g. \"https://workflow-server:7000\") " +
                                 "or 'Elsa:ApiUrl' (e.g. \"https://workflow-server:7000/elsa/api\") in appsettings.json.");

        var apiBaseUri = new Uri(
            resolvedApiUrl.EndsWith('/') ? resolvedApiUrl : $"{resolvedApiUrl}/",
            UriKind.Absolute);

        builder.Services.AddHttpContextAccessor();
        builder.AddTenantIdResolvers();
        builder.Services.TryAddSingleton<ICurrentTenantAccessor, CurrentTenantAccessor>();
        builder.Services.TryAddTransient<Platform.Workflow.Multitenancy.PlatformTenantDelegatingHandler>();

        var redisConfiguration = builder.Configuration["Cache:DistributedGroups:Redis:Default:Configuration"]
                                 ?? throw new InvalidOperationException(
                                     "Distributed Redis configuration is required for standalone workflow idempotency.");
        var workflowStoreOptions = new WorkflowStoreOptions();
        builder.Configuration.GetSection("Workflow:Redis").Bind(workflowStoreOptions);
        builder.Services.TryAddSingleton(workflowStoreOptions);
        builder.Services.TryAddSingleton<IPlatformRedisStore>(_ => new PlatformRedisStore(redisConfiguration));
        builder.Services.TryAddSingleton<IWorkflowIdempotencyStore, PlatformWorkflowIdempotencyStore>();
        builder.Services.TryAddScoped<IWorkflowIdempotencyExecutor, WorkflowIdempotencyExecutor>();

        builder.Services.AddHttpClient<IWorkflowService, RemoteWorkflowService>(client =>
            ConfigureRemoteWorkflowClient(client, apiBaseUri, workflowOptions))
            .AddHttpMessageHandler<Platform.Workflow.Multitenancy.PlatformTenantDelegatingHandler>();

        builder.Services.AddHttpClient<IWorkflowDefinitionService, RemoteWorkflowDefinitionService>(client =>
            ConfigureRemoteWorkflowClient(client, apiBaseUri, workflowOptions))
            .AddHttpMessageHandler<Platform.Workflow.Multitenancy.PlatformTenantDelegatingHandler>();

        builder.Services.AddHttpClient<IWorkflowInstanceService, RemoteWorkflowInstanceService>(client =>
            ConfigureRemoteWorkflowClient(client, apiBaseUri, workflowOptions))
            .AddHttpMessageHandler<Platform.Workflow.Multitenancy.PlatformTenantDelegatingHandler>();

        return builder;
    }

    private static void ConfigureRemoteWorkflowClient(
        HttpClient client,
        Uri apiBaseUri,
        ElsaWorkflowOptions workflowOptions)
    {
        client.BaseAddress = apiBaseUri;

        if (!string.IsNullOrWhiteSpace(workflowOptions.ApiKey))
        {
            client.DefaultRequestHeaders.Add("Authorization", $"ApiKey {workflowOptions.ApiKey}");
        }
    }

    public static IHostApplicationBuilder AddPlatformElsaWorkflow(this IHostApplicationBuilder builder, Action<IModule>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var workflowOptions = ElsaWorkflowOptions.FromConfiguration(builder.Configuration);
        var connectionString = builder.Configuration.GetConnectionString(workflowOptions.ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{workflowOptions.ConnectionStringName}' is not configured or empty.");
        }

        var dbContextOptions = new Elsa.Persistence.EFCore.ElsaDbContextOptions
        {
            SchemaName = workflowOptions.PersistenceSchema,
        };

        builder.Services.AddSingleton(workflowOptions);
        builder.Services.AddHttpContextAccessor();
        builder.AddTenantIdResolvers();
        builder.Services.TryAddSingleton<ICurrentTenantAccessor, CurrentTenantAccessor>();
        builder.Services.AddScoped<IWorkflowService, InProcessWorkflowService>();
        builder.Services.AddScoped<IWorkflowDefinitionService, ElsaWorkflowDefinitionService>();
        builder.Services.AddScoped<IWorkflowInstanceService, ElsaWorkflowInstanceService>();

        // Idempotency: use Redis when configured so multiple workflow nodes share locks and results.
        var redisConfiguration = builder.Configuration["Cache:DistributedGroups:Redis:Default:Configuration"];
        if (!string.IsNullOrWhiteSpace(redisConfiguration))
        {
            var workflowStoreOptions = new WorkflowStoreOptions();
            builder.Configuration.GetSection("Workflow:Redis").Bind(workflowStoreOptions);
            builder.Services.TryAddSingleton(workflowStoreOptions);
            builder.Services.TryAddSingleton<IPlatformRedisStore>(_ => new PlatformRedisStore(redisConfiguration));
            builder.Services.TryAddScoped<IWorkflowIdempotencyStore, PlatformWorkflowIdempotencyStore>();
        }
        else
        {
            // Safe only for a single application instance. Configure Redis for a multi-node deployment.
            builder.Services.TryAddSingleton<IWorkflowIdempotencyStore, MemoryWorkflowIdempotencyStore>();
        }

        builder.Services.TryAddScoped<Platform.Workflow.Idempotency.IWorkflowIdempotencyExecutor, Platform.Workflow.Idempotency.WorkflowIdempotencyExecutor>();

        // Multi-tenancy bridges
        builder.Services.TryAddSingleton<ITenantAccessor, PlatformTenantAccessor>();
        builder.Services.TryAddSingleton<ITenantsProvider, PlatformTenantsProvider>();
        builder.Services.TryAddSingleton<ITenantResolver, PlatformTenantResolver>();

        Elsa.EndpointSecurityOptions.DisableSecurity();

        builder.Services.AddElsa(elsa =>
        {
            elsa.UseTenants();
            elsa.UseJavaScript();

            var module = elsa
                .UseWorkflowManagement(management => management.UseEntityFrameworkCore(ef =>
                {
                    PostgreSqlProvidersExtensions.UsePostgreSql(ef, connectionString, dbContextOptions);
                }))
                .UseWorkflowRuntime(runtime => runtime.UseEntityFrameworkCore(ef =>
                {
                    PostgreSqlProvidersExtensions.UsePostgreSql(ef, connectionString, dbContextOptions);
                }));

            configure?.Invoke(module);
        });

        return builder;
    }
}
