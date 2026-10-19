using Platform.Authentication;
using Platform.Authentication.ApiKey;
using Platform.Authentication.Basic;
using Platform.Authentication.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace UnitTest.Authentication.Helpers;

/// <summary>Test host in-memory với pipeline authentication Platform và endpoint probe.</summary>
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
        bool jwt,
        bool apiKey,
        bool basic = false)
    {
        configuration = MergeSchemeEnableFlags(configuration, jwt, apiKey, basic);

        var host = new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddControllers()
                        .PartManager.ApplicationParts.Add(new AssemblyPart(typeof(TestAuthProbeController).Assembly));

                    var root = configuration.GetSection("Authentication").Get<AuthenticationRootOptions>()
                        ?? new AuthenticationRootOptions();

                    services.AddPlatformAuthentication(
                        configuration,
                        auth =>
                        {
                            if (jwt)
                            {
                                var bearerProfile = AuthenticationSchemeProfileConfiguration.ResolveBearerProfile(root, configuration);
                                auth.AddCoreJwtBearer(
                                    configuration,
                                    PlatformAuthenticationSchemes.Bearer,
                                    bearerProfile);
                            }

                            if (apiKey)
                            {
                                var apiKeyProfile = AuthenticationSchemeProfileConfiguration.ResolveApiKeyProfile(root, configuration);
                                auth.AddCoreApiKeyWithProfileSources<NullApiKeyStorageProvider>(configuration, apiKeyProfile);
                            }

                            if (basic)
                            {
                                var basicProfile = AuthenticationSchemeProfileConfiguration.ResolveBasicProfile(root, configuration);
                                var basicScheme = AuthenticationSchemeProfileConfiguration.ResolveBasicScheme(root, configuration);
                                auth.AddCoreBasicWithProfileSources<NullBasicStorageProvider>(configuration, basicScheme, basicProfile);
                            }
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

    private static IConfiguration MergeSchemeEnableFlags(
        IConfiguration configuration,
        bool jwt,
        bool apiKey,
        bool basic)
    {
        return new ConfigurationBuilder()
            .AddConfiguration(configuration)
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:Schemes:Bearer:Enabled"] = jwt.ToString(),
                ["Authentication:Schemes:ApiKey:Enabled"] = apiKey.ToString(),
                ["Authentication:Schemes:Basic:Enabled"] = basic.ToString(),
            })
            .Build();
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
