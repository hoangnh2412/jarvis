using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Platform.DDD.Domain.Services;
using Platform.MultiOrg;
using Platform.MultiOrg.Middleware;
using Platform.Multitenancy;
using Platform.Mvc.ExceptionHandling;

namespace UnitTest.MultiOrg;

/// <summary>Pipeline <c>UseCoreMiddleware&lt;MultiOrgMiddleware&gt;</c> với <c>Middlewares:MultiOrg</c> giống Sample.</summary>
public sealed class MultiOrgPathFilterTests
{
    [Theory]
    [InlineData("/health", StatusCodes.Status204NoContent, 0)]
    [InlineData("/", StatusCodes.Status204NoContent, 0)]
    [InlineData("/api/v1/orders", StatusCodes.Status403Forbidden, 1)]
    [InlineData("/hubs/notify", StatusCodes.Status403Forbidden, 1)]
    [InlineData("/sse/events", StatusCodes.Status403Forbidden, 1)]
    [InlineData("/elsa/workflow", StatusCodes.Status403Forbidden, 1)]
    [InlineData("/api/v1/authentication/login", StatusCodes.Status204NoContent, 0)]
    [InlineData("/api/v1/system/install", StatusCodes.Status204NoContent, 0)]
    [InlineData("/api/v1/platform/tenants", StatusCodes.Status204NoContent, 0)]
    [InlineData("/api/v1/foo/account/login", StatusCodes.Status204NoContent, 0)]
    [InlineData("/api/v1/foo/account/catalog", StatusCodes.Status204NoContent, 0)]
    [InlineData("/api/v1/account/login", StatusCodes.Status403Forbidden, 1)]
    [InlineData("/api/authentication", StatusCodes.Status403Forbidden, 1)]
    public async Task Sample_rules_run_resolver_only_for_scoped_paths(string path, int status, int resolveCalls)
    {
        await using var host = await CreateHostAsync();
        var client = host.GetTestClient();

        var response = await client.GetAsync(path);

        Assert.Equal(status, (int)response.StatusCode);
        Assert.Equal(resolveCalls, host.Services.GetRequiredService<ResolveCallCount>().Value);
    }

    private static async Task<WebApplication> CreateHostAsync()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Development,
        });
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(SampleMultiOrgSection());
        builder.Services.AddSingleton<ResolveCallCount>();
        builder.Services.AddScoped<IRequestScopeResolver, ForbiddenScopeResolver>();
        builder.Services.AddSingleton<ICurrentOrgAccessor, CurrentOrgAccessor>();
        builder.Services.AddSingleton<ICurrentTenantAccessor, CurrentTenantAccessor>();

        var app = builder.Build();
        app.UseCoreMiddleware<MultiOrgMiddleware>();
        app.Run(ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status204NoContent;
            return Task.CompletedTask;
        });
        await app.StartAsync();
        return app;
    }

    private static Dictionary<string, string?> SampleMultiOrgSection() => new()
    {
        ["Middlewares:MultiOrg:IsEnable"] = "true",
        ["Middlewares:MultiOrg:Includes:0"] = "^/api/",
        ["Middlewares:MultiOrg:Includes:1"] = "^/hubs/",
        ["Middlewares:MultiOrg:Includes:2"] = "^/sse/",
        ["Middlewares:MultiOrg:Includes:3"] = "^/elsa/",
        ["Middlewares:MultiOrg:Excludes:0"] = @"^/api/v\d+(?:\.\d+)?/authentication",
        ["Middlewares:MultiOrg:Excludes:1"] = @"^/api/v\d+(?:\.\d+)?/system/install",
        ["Middlewares:MultiOrg:Excludes:2"] = @"^/api/v\d+(?:\.\d+)?/platform/",
        ["Middlewares:MultiOrg:Excludes:3"] = @"^/api/v\d+(?:\.\d+)?/.*/account/login",
        ["Middlewares:MultiOrg:Excludes:4"] = @"^/api/v\d+(?:\.\d+)?/.*/account/catalog",
    };

    private sealed class ResolveCallCount
    {
        public int Value;
    }

    private sealed class ForbiddenScopeResolver(ResolveCallCount calls) : IRequestScopeResolver
    {
        public Task<RequestScopeResolution> ResolveAsync(CancellationToken cancellationToken = default)
        {
            calls.Value++;
            return Task.FromResult(RequestScopeResolution.Forbidden);
        }
    }
}
