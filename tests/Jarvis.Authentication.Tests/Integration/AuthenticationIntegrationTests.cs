using System.Text.Json;
using Jarvis.Authentication.ApiKey;
using Jarvis.Authentication.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.Authentication.Tests.Integration;

public class AuthenticationIntegrationTests
{
    [Fact]
    public async Task AK_I_01_Valid_api_key_authenticates()
    {
        var config = AuthenticationConfigurationBuilder.BuildApiKeyConfig(keys: "test-secret-key");
        await using var server = await AuthenticationTestServer.CreateAsync(config, composite: false, jwt: false, apiKey: true);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/_auth-test/whoami");
        request.Headers.Add("X-API-KEY", "test-secret-key");

        var response = await server.Client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        await AssertAuthenticatedAsync(response, expected: true);
    }

    [Fact]
    public async Task AK_I_02_Missing_or_wrong_api_key_not_authenticated()
    {
        var config = AuthenticationConfigurationBuilder.BuildApiKeyConfig(keys: "test-secret-key");
        await using var server = await AuthenticationTestServer.CreateAsync(config, composite: false, jwt: false, apiKey: true);

        var noHeader = await server.Client.GetAsync("/api/_auth-test/whoami");
        noHeader.EnsureSuccessStatusCode();
        await AssertAuthenticatedAsync(noHeader, expected: false);

        var wrong = new HttpRequestMessage(HttpMethod.Get, "/api/_auth-test/whoami");
        wrong.Headers.Add("X-API-KEY", "wrong");
        var wrongResponse = await server.Client.SendAsync(wrong);
        wrongResponse.EnsureSuccessStatusCode();
        await AssertAuthenticatedAsync(wrongResponse, expected: false);
    }

    [Fact]
    public void AK_I_04_AddCoreApiKey_registers_provider()
    {
        var config = AuthenticationConfigurationBuilder.BuildApiKeyConfig(keys: "k");
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddJarvisAuthentication(config, auth => auth.AddCoreApiKey(config));
        var sp = services.BuildServiceProvider();

        var provider = sp.GetRequiredService<AspNetCore.Authentication.ApiKey.IApiKeyProvider>();
        Assert.IsType<Jarvis.Authentication.ApiKey.ApiKeyProvider>(provider);
    }

    [Fact]
    public async Task MIX_I_01_Composite_forwards_api_key()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:Type"] = "ApiKey",
                ["Authentication:DefaultAuthenticateScheme"] = "Composite",
                ["Authentication:DefaultChallengeScheme"] = "Composite",
                ["Authentication:Schemes:Jwt:Enabled"] = "true",
                ["Authentication:Schemes:ApiKey:Enabled"] = "true",
                ["Authentication:ApiKey:Default:KeyName"] = "X-API-KEY",
                ["Authentication:ApiKey:Default:Mode"] = "SingleKey",
                ["Authentication:ApiKey:Default:Keys:0"] = "mix-secret",
                ["Authentication:Jwt:Bearer:IssuerSigningKeys:0"] = "test-signing-key-at-least-32-chars-long",
                ["Authentication:Jwt:Bearer:ValidateAudience"] = "false",
                ["Authentication:Jwt:Bearer:RequireHttpsMetadata"] = "false",
            })
            .Build();

        await using var server = await AuthenticationTestServer.CreateAsync(config, composite: true, jwt: true, apiKey: true, basic: false);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/_auth-test/whoami");
        request.Headers.Add("X-API-KEY", "mix-secret");

        var response = await server.Client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        await AssertAuthenticatedAsync(response, expected: true);
    }

    [Fact]
    public void CFG_01_Sample_appsettings_has_no_plaintext_api_keys()
    {
        var repoRoot = FindRepoRoot();
        var path = Path.Combine(repoRoot, "Sample", "appsettings.json");
        var json = File.ReadAllText(path);

        Assert.DoesNotContain("ZofkXgxwiO6F2s1JJCX5L6Wa7JctPmpO", json);
        Assert.DoesNotMatch("\"Keys\"\\s*:\\s*\\[\\s*\"[^\"]+\"", json);
    }

    private static async Task AssertAuthenticatedAsync(HttpResponseMessage response, bool expected)
    {
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var authenticated = doc.RootElement.GetProperty("authenticated").GetBoolean();
        Assert.Equal(expected, authenticated);
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "Jarvis.sln")))
            dir = dir.Parent;

        return dir?.FullName ?? throw new InvalidOperationException("Could not find repo root.");
    }
}
