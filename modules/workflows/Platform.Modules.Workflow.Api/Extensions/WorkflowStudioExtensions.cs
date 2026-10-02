using Elsa.Studio.Core.BlazorServer.Extensions;
using Elsa.Studio.Authentication.Abstractions.Contracts;
using Elsa.Studio.Authentication.Abstractions.ComponentProviders;
using Elsa.Studio.Contracts;
using Elsa.Studio.Extensions;
using Elsa.Studio.Models;
using Elsa.Studio.Shell.Extensions;
using Elsa.Studio.Workflows.Extensions;
using Elsa.Studio.Workflows.Designer.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Platform.Modules.ElsaWorkflow.Api.Authentication;
using Platform.Modules.ElsaWorkflow.Api.Components;
using Platform.Modules.ElsaWorkflow.Extensions;
using Platform.Workflow.Multitenancy;
using Platform.Workflow.Configuration;
using MudBlazor.Services;

namespace Platform.Modules.ElsaWorkflow.Api.Extensions;

public static class WorkflowStudioExtensions
{
    public static ElsaWorkflowModuleBuilder UseStudio(this ElsaWorkflowModuleBuilder builder, string? backendApiUrl = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (builder.HostBuilder is not WebApplicationBuilder webBuilder)
            throw new InvalidOperationException("Elsa Studio requires WebApplicationBuilder host.");

        var workflowOptions = ElsaWorkflowOptions.FromConfiguration(webBuilder.Configuration);

        webBuilder.WebHost.UseStaticWebAssets();

        webBuilder.Services.AddRazorPages()
            .AddApplicationPart(typeof(WorkflowStudioExtensions).Assembly);
        webBuilder.Services.AddServerSideBlazor(options =>
        {
            options.DetailedErrors = string.Equals(webBuilder.Environment.EnvironmentName,"Development",StringComparison.OrdinalIgnoreCase);
            options.RootComponents.RegisterCustomElsaStudioElements();
            options.RootComponents.MaxJSRootComponents = 1000; // tăng kích thước để hỗ trợ nhiều component hơn trong Elsa Studio
        });

        // Tăng giới hạn kích thước message nhận qua SignalR (mặc định là 32 KB) lên 10 MB để hỗ trợ Elsa Studio (Blazor Server)
        // truyền tải các workflow definition lớn, đồ thị designer phức tạp và dữ liệu JSInterop mà không bị ngắt kết nối WebSocket.
        webBuilder.Services.AddSignalR(options =>
        {
            options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10 MB
        });

        webBuilder.Services.AddCore();
        webBuilder.Services.AddMudServices();
        webBuilder.Services.AddShell(options => options.DisableAuthorization = true);
        webBuilder.Services.AddScoped<IUnauthorizedComponentProvider,
            UnauthorizedComponentProvider<WorkflowUnauthorizedComponent>>();

        var resolvedApiUrl = backendApiUrl
            ?? workflowOptions.GetResolvedApiUrl()
            ?? ResolveSameHostElsaApiUrl(webBuilder, workflowOptions.ApiPathPrefix)
            ?? $"http://localhost:5167{workflowOptions.ApiPathPrefix.TrimEnd('/')}";

        webBuilder.Services.AddRemoteBackend(new BackendApiConfig
        {
            ConfigureBackendOptions = options => options.Url = new Uri(resolvedApiUrl),
        });

        webBuilder.Services.AddTransient<WorkflowPreviewHttpMessageHandler>();
        webBuilder.Services.TryAddTransient<PlatformTenantDelegatingHandler>();
        webBuilder.Services.AddHttpClient("WorkflowPreview", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddHttpMessageHandler<WorkflowPreviewHttpMessageHandler>()
            .AddHttpMessageHandler<PlatformTenantDelegatingHandler>();

        webBuilder.Services.AddWorkflowsModule();
        webBuilder.Services.AddScoped<IHttpConnectionOptionsConfigurator, WorkflowStudioHttpConnectionOptionsConfigurator>();
        webBuilder.Services.AddHttpContextAccessor();

        return builder;
    }

    private static string? ResolveSameHostElsaApiUrl(WebApplicationBuilder webBuilder, string apiPathPrefix)
    {
        var urls = webBuilder.WebHost.GetSetting(WebHostDefaults.ServerUrlsKey)
                   ?? webBuilder.Configuration["ASPNETCORE_URLS"]
                   ?? webBuilder.Configuration["urls"];
        if (string.IsNullOrWhiteSpace(urls))
            return null;

        var hostUrl = urls
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(hostUrl))
            return null;

        var prefix = string.IsNullOrWhiteSpace(apiPathPrefix) ? "/elsa/api" : (apiPathPrefix.StartsWith('/') ? apiPathPrefix : $"/{apiPathPrefix}");
        return $"{hostUrl.TrimEnd('/')}{prefix}";
    }

    public static WebApplication UseWorkflowStudio(this WebApplication app, string? loginPath = null)
    {
        ArgumentNullException.ThrowIfNull(app);

        var services = ((IApplicationBuilder)app).ApplicationServices;
        var options = services.GetService<ElsaWorkflowOptions>()
            ?? ElsaWorkflowOptions.FromConfiguration(services.GetRequiredService<IConfiguration>());

        var studioPrefixes = options.StudioPathPrefixes.Length > 0
            ? options.StudioPathPrefixes
            : ["/workflows", "/workflow-definitions", "/workflow-instances", "/studio", "/preview"];

        var authorizationPolicy = options.StudioAuthorizationPolicy ?? options.AuthorizationPolicy;
        app.UseWhen(
            context => studioPrefixes.Any(prefix =>
                context.Request.Path.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase)),
            branch => branch.Use(async (context, next) =>
            {
                if (string.IsNullOrWhiteSpace(authorizationPolicy))
                {
                    await next();
                    return;
                }

                var authorization = context.RequestServices.GetRequiredService<IAuthorizationService>();
                var result = await authorization.AuthorizeAsync(
                    context.User,
                    resource: null,
                    policyName: authorizationPolicy);

                if (result.Succeeded)
                {
                    await next();
                    return;
                }

                if (context.User.Identity?.IsAuthenticated == true)
                    await context.ForbidAsync();
                else
                    await context.ChallengeAsync();
            }));

        _ = loginPath; // Authentication redirect belongs to the host application.

        app.UseStaticFiles();
        app.MapStaticAssets();
        app.MapRazorPages();
        app.MapBlazorHub();

        foreach (var prefix in studioPrefixes)
        {
            var trimmedPrefix = prefix.TrimEnd('/');
            if (string.Equals(trimmedPrefix, "/preview", StringComparison.OrdinalIgnoreCase))
                continue; // /preview has its own Razor page Preview.cshtml

            var fallback = app.MapFallbackToPage($"{trimmedPrefix}/{{*path}}", "/_Host");
            if (!string.IsNullOrWhiteSpace(authorizationPolicy))
            {
                fallback.RequireAuthorization(authorizationPolicy);
            }

            var rootFallback = app.MapFallbackToPage(trimmedPrefix, "/_Host");
            if (!string.IsNullOrWhiteSpace(authorizationPolicy))
            {
                rootFallback.RequireAuthorization(authorizationPolicy);
            }
        }

        return app;
    }
}
