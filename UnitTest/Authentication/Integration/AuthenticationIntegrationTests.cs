using System.Text.Json;
using Platform.Authentication;
using Platform.Authentication.ApiKey;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UnitTest.Authentication.Helpers;

namespace UnitTest.Authentication.Integration;

/// <summary>Integration test end-to-end — HTTP qua <see cref="AuthenticationTestServer"/>.</summary>
public class AuthenticationIntegrationTests
{
    /// <summary>Header <c>X-API-KEY</c> đúng — user được authenticate.</summary>
    [Fact]
    public async Task AK_I_01_Valid_api_key_authenticates()
    {
        var config = AuthenticationConfigurationBuilder.BuildApiKeyConfig(key: "test-secret-key");
        await using var server = await AuthenticationTestServer.CreateAsync(config, jwt: false, apiKey: true);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/_auth-test/whoami");
        request.Headers.Add("X-API-KEY", "test-secret-key");

        var response = await server.Client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        await AssertAuthenticatedAsync(response, expected: true);
    }

    /// <summary>Thiếu header hoặc key sai — không authenticate.</summary>
    [Fact]
    public async Task AK_I_02_Missing_or_wrong_api_key_not_authenticated()
    {
        var config = AuthenticationConfigurationBuilder.BuildApiKeyConfig(key: "test-secret-key");
        await using var server = await AuthenticationTestServer.CreateAsync(config, jwt: false, apiKey: true);

        var noHeader = await server.Client.GetAsync("/api/_auth-test/whoami");
        noHeader.EnsureSuccessStatusCode();
        await AssertAuthenticatedAsync(noHeader, expected: false);

        var wrong = new HttpRequestMessage(HttpMethod.Get, "/api/_auth-test/whoami");
        wrong.Headers.Add("X-API-KEY", "wrong");
        var wrongResponse = await server.Client.SendAsync(wrong);
        wrongResponse.EnsureSuccessStatusCode();
        await AssertAuthenticatedAsync(wrongResponse, expected: false);
    }

    /// <summary><c>AddCoreApiKey</c> đăng ký <see cref="ConfigApiKeyProvider"/> vào DI.</summary>
    [Fact]
    public void AK_I_04_AddCoreApiKey_registers_provider()
    {
        var config = AuthenticationConfigurationBuilder.BuildApiKeyConfig(key: "k");
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPlatformAuthentication(config, auth => auth.AddCoreApiKey<ConfigApiKeyProvider>(config));
        var sp = services.BuildServiceProvider();

        var provider = sp.GetRequiredService<AspNetCore.Authentication.ApiKey.IApiKeyProvider>();
        Assert.IsType<ConfigApiKeyProvider>(provider);
    }

    /// <summary>Default scheme ApiKey — request có <c>X-API-KEY</c> được authenticate.</summary>
    [Fact]
    public async Task MIX_I_01_Default_scheme_api_key_authenticates()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:DefaultAuthenticateScheme"] = "ApiKey",
                ["Authentication:DefaultChallengeScheme"] = "ApiKey",
                ["Authentication:Schemes:ApiKey:Enabled"] = "true",
                ["Authentication:Schemes:ApiKey:Profiles:Default:Source"] = "Configuration",
                ["Authentication:Schemes:ApiKey:Profiles:Default:KeyName"] = "X-API-KEY",
                ["Authentication:Schemes:ApiKey:Profiles:Default:Key"] = "mix-secret",
            })
            .Build();

        await using var server = await AuthenticationTestServer.CreateAsync(config, jwt: false, apiKey: true, basic: false);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/_auth-test/whoami");
        request.Headers.Add("X-API-KEY", "mix-secret");

        var response = await server.Client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        await AssertAuthenticatedAsync(response, expected: true);
    }

    /// <summary>Sample <c>appsettings.json</c> không chứa API key plaintext (dùng User Secrets).</summary>
    [Fact]
    public void CFG_01_Sample_appsettings_has_no_plaintext_api_keys()
    {
        var repoRoot = FindRepoRoot();
        var path = Path.Combine(repoRoot, "Sample", "appsettings.json");
        var json = File.ReadAllText(path);

        Assert.DoesNotContain("ZofkXgxwiO6F2s1JJCX5L6Wa7JctPmpO", json);

        var config = new ConfigurationBuilder().AddJsonFile(path, optional: false).Build();
        Assert.Equal("X-API-KEY", config["Authentication:Schemes:ApiKey:Profiles:Default:KeyName"]);
        Assert.Null(config["Authentication:Schemes:ApiKey:Profiles:Default:Key"]);
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
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "Platform.sln")))
            dir = dir.Parent;

        return dir?.FullName ?? throw new InvalidOperationException("Could not find repo root.");
    }
}
