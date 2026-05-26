using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Jarvis.Authentication.Basic;
using Jarvis.Authentication.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Jarvis.Authentication.Tests.Basic;

/// <summary>Integration test HTTP Basic Authentication qua TestServer.</summary>
public class BasicAuthenticationTests
{
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

    [Fact]
    public void BASIC_U_01_Config_validator_requires_users()
    {
        var validator = new AuthenticationBasicOptionValidator();
        var result = validator.Validate("Basic", new AuthenticationBasicOption());
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void BASIC_I_03_AddCoreBasic_registers_validator()
    {
        var config = AuthenticationConfigurationBuilder.BuildBasicConfig();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddJarvisAuthentication(config, auth => auth.AddCoreBasic(config));
        var sp = services.BuildServiceProvider();

        Assert.IsType<ConfigBasicCredentialValidator>(sp.GetRequiredService<IBasicCredentialValidator>());
    }

    [Fact]
    public async Task BASIC_U_02_Delegate_lookup_validates_credentials()
    {
        BasicCredentialLookupAsync lookup = (scheme, username, _) =>
            Task.FromResult<BasicUserCredential?>(
                username == "dbuser"
                    ? new BasicUserCredential { Password = "dbpass", Roles = ["admin"] }
                    : null);

        var validator = new DelegateBasicCredentialValidator(lookup);

        var valid = await validator.ValidateAsync("Basic", "dbuser", "dbpass");
        Assert.NotNull(valid);
        Assert.Equal("dbuser", valid!.Username);
        Assert.Contains(valid.Claims, c => c.Type == ClaimTypes.Role && c.Value == "admin");

        var invalid = await validator.ValidateAsync("Basic", "dbuser", "wrong");
        Assert.Null(invalid);
    }

    [Fact]
    public void BASIC_U_03_Delegate_path_allows_empty_config_users()
    {
        var validator = new AuthenticationBasicOptionValidator(requireUsers: false);
        var result = validator.Validate("Basic", new AuthenticationBasicOption());
        Assert.True(result.Succeeded);
    }

    [Fact]
    public void BASIC_I_04_AddCoreBasic_delegate_registers_validator()
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
            auth.AddCoreBasic(config, (_, _, _) => Task.FromResult<BasicUserCredential?>(null)));

        var sp = services.BuildServiceProvider();
        Assert.IsType<DelegateBasicCredentialValidator>(sp.GetRequiredService<IBasicCredentialValidator>());
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
