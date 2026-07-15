using System.Text.Json;
using Jarvis.Authentication;
using Jarvis.Authentication.ApiKey;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
        await using var server = await AuthenticationTestServer.CreateAsync(config, composite: false, jwt: false, apiKey: true);

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

    /// <summary><c>AddCoreApiKey</c> đăng ký <see cref="ConfigApiKeyProvider"/> vào DI.</summary>
    [Fact]
    public void AK_I_04_AddCoreApiKey_registers_provider()
    {
        var config = AuthenticationConfigurationBuilder.BuildApiKeyConfig(key: "k");
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddJarvisAuthentication(config, auth => auth.AddCoreApiKey<ConfigApiKeyProvider>(config));
        var sp = services.BuildServiceProvider();

        var provider = sp.GetRequiredService<AspNetCore.Authentication.ApiKey.IApiKeyProvider>();
        Assert.IsType<ConfigApiKeyProvider>(provider);
    }

    /// <summary>Custom <see cref="IApiKeyProvider"/> + <c>Key</c> rỗng — startup OK (chỉ bắt buộc <c>KeyName</c>).</summary>
    [Fact]
    public void AK_I_05_Custom_provider_allows_empty_config_key_at_startup()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:ApiKey:Default:KeyName"] = "X-API-KEY",
                ["Authentication:ApiKey:Default:Key"] = "",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddJarvisAuthentication(config, auth => auth.AddCoreApiKey<NoOpApiKeyProvider>(config));
        var sp = services.BuildServiceProvider();

        // Force ValidateOnStart for named AuthenticationApiKeyOption.
        var options = sp.GetRequiredService<IOptionsMonitor<AuthenticationApiKeyOption>>().Get("Default");
        Assert.Equal("X-API-KEY", options.KeyName);
        Assert.True(string.IsNullOrEmpty(options.Key));
        Assert.IsType<NoOpApiKeyProvider>(sp.GetRequiredService<AspNetCore.Authentication.ApiKey.IApiKeyProvider>());
        // RequireConfigKey giờ gắn theo từng realm (named) — custom provider ⇒ false cho realm "Default".
        Assert.False(sp.GetRequiredService<IOptionsMonitor<ApiKeyProviderOptions>>().Get("Default").RequireConfigKey);
    }

    /// <summary><see cref="ConfigApiKeyProvider"/> + <c>Key</c> rỗng — ValidateOnStart fail.</summary>
    [Fact]
    public void AK_I_06_Config_provider_empty_key_fails_startup_validation()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:ApiKey:Default:KeyName"] = "X-API-KEY",
                ["Authentication:ApiKey:Default:Key"] = "",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddJarvisAuthentication(config, auth => auth.AddCoreApiKey<ConfigApiKeyProvider>(config));
        var sp = services.BuildServiceProvider();

        var ex = Assert.Throws<OptionsValidationException>(() =>
            sp.GetRequiredService<IOptionsMonitor<AuthenticationApiKeyOption>>().Get("Default"));
        Assert.Contains("Key is required", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class NoOpApiKeyProvider : AspNetCore.Authentication.ApiKey.IApiKeyProvider
    {
        public Task<AspNetCore.Authentication.ApiKey.IApiKey?> ProvideAsync(string key) =>
            Task.FromResult<AspNetCore.Authentication.ApiKey.IApiKey?>(null);
    }

    /// <summary>Scheme <c>Composite</c> — request có <c>X-API-KEY</c> forward sang ApiKey.</summary>
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
                ["Authentication:ApiKey:Default:Key"] = "mix-secret",
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

    /// <summary>Sample <c>appsettings.json</c> không chứa API key plaintext (dùng User Secrets).</summary>
    [Fact]
    public void CFG_01_Sample_appsettings_has_no_plaintext_api_keys()
    {
        var repoRoot = FindRepoRoot();
        var path = Path.Combine(repoRoot, "Sample", "appsettings.json");
        var json = File.ReadAllText(path);

        Assert.DoesNotContain("ZofkXgxwiO6F2s1JJCX5L6Wa7JctPmpO", json);

        var config = new ConfigurationBuilder().AddJsonFile(path, optional: false).Build();
        Assert.True(string.IsNullOrEmpty(config["Authentication:ApiKey:Default:Key"]));
        Assert.True(string.IsNullOrEmpty(config["Authentication:ApiKey:Integration:Key"]));
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
