using Jarvis.Authentication.ApiKey;
using Jarvis.Authentication.Basic;
using Jarvis.Authentication.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Jarvis.Authentication.Tests.Helpers;

/// <summary>Test host in-memory với pipeline authentication Jarvis và endpoint probe.</summary>
public sealed class AuthenticationTestServer : IAsyncDisposable
{
    private readonly IHost _host;

    public HttpClient Client { get; }

    private AuthenticationTestServer(IHost host, HttpClient client)
    {
        _host = host;
        Client = client;
    }

    public static async Task<AuthenticationTestServer> CreateAsync(
        IConfiguration configuration,
        bool composite,
        bool jwt,
        bool apiKey,
        bool basic = false)
    {
        var host = new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddControllers()
                        .PartManager.ApplicationParts.Add(new AssemblyPart(typeof(TestAuthProbeController).Assembly));

                    services.AddJarvisAuthentication(configuration, auth =>
                    {
                        if (composite)
                            auth.AddJarvisCompositeScheme(includeBasic: basic);

                        if (jwt)
                            auth.AddCoreJwtBearer(configuration, JwtBearerDefaults.AuthenticationScheme);

                        if (apiKey)
                            auth.AddCoreApiKey(configuration, JarvisAuthenticationSchemes.ApiKey);

                        if (basic)
                            auth.AddCoreBasic(configuration, JarvisAuthenticationSchemes.Basic);
                    });
                });
                webBuilder.Configure(app =>
                {
                    app.UseRouting();
                    app.UseAuthentication();
                    app.UseAuthorization();
                    app.UseEndpoints(endpoints => endpoints.MapControllers());
                });
            })
            .Build();

        await host.StartAsync().ConfigureAwait(false);
        return new AuthenticationTestServer(host, host.GetTestClient());
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _host.StopAsync();
        _host.Dispose();
    }
}

[ApiController]
[Route("api/_auth-test")]
public sealed class TestAuthProbeController : ControllerBase
{
    [HttpGet("whoami")]
    [AllowAnonymous]
    public IActionResult WhoAmI() =>
        Ok(new
        {
            Authenticated = User.Identity?.IsAuthenticated ?? false,
            Scheme = User.Identity?.AuthenticationType,
            Name = User.Identity?.Name
        });
}
