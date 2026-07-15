using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Jarvis.Authentication;
using Jarvis.Authentication.Basic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UnitTest.Authentication.Helpers;

namespace UnitTest.Authentication.Basic;

/// <summary>Test HTTP Basic Authentication qua TestServer.</summary>
public class BasicAuthenticationTests
{
    /// <summary>User/password đúng trong config — request được authenticate.</summary>
    [Fact]
    public async Task BASIC_I_01_Valid_credentials_authenticate()
    {
        var config = AuthenticationConfigurationBuilder.BuildBasicConfig();
        await using var server = await AuthenticationTestServer.CreateAsync(config, composite: false, jwt: false, apiKey: false, basic: true);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/_auth-test/whoami");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes("testuser:testpass")));

        var response = await server.Client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        await AssertAuthenticatedAsync(response, expected: true, expectedName: "testuser");
    }

    /// <summary>Thiếu header hoặc password sai — không authenticate.</summary>
    [Fact]
    public async Task BASIC_I_02_Invalid_or_missing_credentials_not_authenticated()
    {
        var config = AuthenticationConfigurationBuilder.BuildBasicConfig();
        await using var server = await AuthenticationTestServer.CreateAsync(config, composite: false, jwt: false, apiKey: false, basic: true);

        var noHeader = await server.Client.GetAsync("/api/_auth-test/whoami");
        noHeader.EnsureSuccessStatusCode();
        await AssertAuthenticatedAsync(noHeader, expected: false);

        var wrong = new HttpRequestMessage(HttpMethod.Get, "/api/_auth-test/whoami");
        wrong.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes("testuser:wrong")));
        var wrongResponse = await server.Client.SendAsync(wrong);
        wrongResponse.EnsureSuccessStatusCode();
        await AssertAuthenticatedAsync(wrongResponse, expected: false);
    }

    /// <summary>Policy <c>Composite</c> — forward sang Basic khi header <c>Authorization: Basic</c>.</summary>
    [Fact]
    public async Task MIX_I_02_Composite_forwards_basic()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:DefaultAuthenticateScheme"] = "Composite",
                ["Authentication:DefaultChallengeScheme"] = "Composite",
                ["Authentication:Basic:Default:Realm"] = "Test",
                ["Authentication:Basic:Default:Users:mix:Password"] = "mix-pass",
                ["Authentication:Basic:Default:Users:mix:Roles:0"] = "mix",
            })
            .Build();

        await using var server = await AuthenticationTestServer.CreateAsync(
            config,
            composite: true,
            jwt: false,
            apiKey: false,
            basic: true);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/_auth-test/whoami");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes("mix:mix-pass")));

        var response = await server.Client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        await AssertAuthenticatedAsync(response, expected: true, expectedName: "mix");
    }

    /// <summary><c>AddCoreBasic(configuration)</c> đăng ký <see cref="ConfigBasicCredentialProvider"/> mặc định.</summary>
    [Fact]
    public void BASIC_I_03_AddCoreBasic_registers_config_provider()
    {
        var config = AuthenticationConfigurationBuilder.BuildBasicConfig();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddJarvisAuthentication(config, auth => auth.AddCoreBasic<ConfigBasicCredentialProvider>(config));
        var sp = services.BuildServiceProvider();

        Assert.IsType<ConfigBasicCredentialProvider>(sp.GetRequiredService<IBasicCredentialProvider>());
    }

    /// <summary>Helper so password — build claims Name và Roles.</summary>
    [Fact]
    public void BASIC_U_02_Credential_validation_helper_builds_claims()
    {
        var credential = new BasicUserCredential { Password = "dbpass", Roles = ["admin"] };

        var valid = BasicValidationResult.Validate("dbuser", "dbpass", credential);
        Assert.NotNull(valid);
        Assert.Equal("dbuser", valid!.Username);
        Assert.Contains(valid.Claims, c => c.Type == ClaimTypes.Role && c.Value == "admin");

        Assert.Null(BasicValidationResult.Validate("dbuser", "wrong", credential));
    }

    /// <summary><c>AddCoreBasic&lt;TCredentialProvider&gt;</c> — đăng ký implementation host vào DI.</summary>
    [Fact]
    public void BASIC_I_04_AddCoreBasic_generic_registers_provider()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:Basic:Default:Realm"] = "Test",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddJarvisAuthentication(config, auth =>
            auth.AddCoreBasic<NoOpBasicCredentialProvider>(config));

        var sp = services.BuildServiceProvider();
        Assert.IsType<NoOpBasicCredentialProvider>(sp.GetRequiredService<IBasicCredentialProvider>());
    }

    private sealed class NoOpBasicCredentialProvider : IBasicCredentialProvider
    {
        public Task<BasicValidationResult?> AuthenticateAsync(
            string schemeName,
            string username,
            string password,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<BasicValidationResult?>(null);
    }

    private static async Task AssertAuthenticatedAsync(HttpResponseMessage response, bool expected, string? expectedName = null)
    {
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var authenticated = doc.RootElement.GetProperty("authenticated").GetBoolean();
        Assert.Equal(expected, authenticated);

        if (expectedName is not null)
        {
            var name = doc.RootElement.GetProperty("name").GetString();
            Assert.Equal(expectedName, name);
        }
    }
}
