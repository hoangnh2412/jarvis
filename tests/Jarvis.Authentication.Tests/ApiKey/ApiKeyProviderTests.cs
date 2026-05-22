using AspNetCore.Authentication.ApiKey;
using Jarvis.Authentication.ApiKey;
using Microsoft.Extensions.Configuration;
using Jarvis.Authentication.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Jarvis.Authentication.Tests.ApiKey;

public class ApiKeyProviderTests
{
    private static ApiKeyProvider CreateProvider(IConfiguration configuration, string scheme = "Default")
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var section = configuration.GetSection($"Authentication:ApiKey:{scheme}");
        services.Configure<AuthenticationApiKeyOption>(scheme, section);
        services.AddOptions<AuthenticationApiKeyOption>(scheme).Bind(section);
        services.AddSingleton<IPostConfigureOptions<AuthenticationApiKeyOption>, AuthenticationApiKeyPostConfigureOptions>();
        services.Configure<ApiKeyProviderOptions>(o => o.DefaultSchemeName = scheme);

        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IOptionsFactory<AuthenticationApiKeyOption>>();
        var post = provider.GetRequiredService<IPostConfigureOptions<AuthenticationApiKeyOption>>();
        post.PostConfigure(scheme, factory.Create(scheme));

        return new ApiKeyProvider(
            factory,
            provider.GetRequiredService<IOptions<ApiKeyProviderOptions>>(),
            NullLogger<ApiKeyProvider>.Instance);
    }

    [Fact]
    public async Task AK_U_01_SingleKey_valid_returns_api_key()
    {
        var config = AuthenticationConfigurationBuilder.BuildApiKeyConfig(keys: "secret");
        var provider = CreateProvider(config);

        var result = await provider.ProvideAsync("secret");

        Assert.NotNull(result);
        Assert.Equal("Default", result!.OwnerName);
    }

    [Fact]
    public async Task AK_U_02_SingleKey_wrong_returns_null()
    {
        var provider = CreateProvider(AuthenticationConfigurationBuilder.BuildApiKeyConfig(keys: "secret"));

        Assert.Null(await provider.ProvideAsync("wrong"));
    }

    [Fact]
    public async Task AK_U_03_SingleKey_realm_format_returns_null()
    {
        var provider = CreateProvider(AuthenticationConfigurationBuilder.BuildApiKeyConfig(keys: "secret"));

        Assert.Null(await provider.ProvideAsync("Default:secret"));
    }

    [Fact]
    public async Task AK_U_04_RealmKey_valid_returns_api_key()
    {
        var config = AuthenticationConfigurationBuilder.BuildApiKeyConfig(mode: "RealmKey", keys: "s1");
        var provider = CreateProvider(config);

        var result = await provider.ProvideAsync("Default:s1");

        Assert.NotNull(result);
    }

    [Fact]
    public async Task AK_U_05_RealmKey_without_colon_returns_null()
    {
        var config = AuthenticationConfigurationBuilder.BuildApiKeyConfig(mode: "RealmKey", keys: "s1");
        var provider = CreateProvider(config);

        Assert.Null(await provider.ProvideAsync("s1"));
    }

    [Fact]
    public async Task AK_U_06_RealmKey_unknown_realm_returns_null()
    {
        var config = AuthenticationConfigurationBuilder.BuildApiKeyConfig(mode: "RealmKey", keys: "s1");
        var provider = CreateProvider(config);

        Assert.Null(await provider.ProvideAsync("Other:s1"));
    }

    [Fact]
    public async Task AK_U_07_Large_key_list_resolves_correct_key()
    {
        var keys = Enumerable.Range(0, 100).Select(i => $"key-{i}").ToArray();
        var data = new Dictionary<string, string?>
        {
            ["Authentication:ApiKey:Default:KeyName"] = "X-API-KEY",
            ["Authentication:ApiKey:Default:Mode"] = "SingleKey",
        };
        for (var i = 0; i < keys.Length; i++)
            data[$"Authentication:ApiKey:Default:Keys:{i}"] = keys[i];

        var provider = CreateProvider(new ConfigurationBuilder().AddInMemoryCollection(data).Build());

        Assert.NotNull(await provider.ProvideAsync("key-50"));
        Assert.Null(await provider.ProvideAsync("key-missing"));
    }

    [Fact]
    public void AK_U_08_Empty_keys_fails_validation()
    {
        var validator = new AuthenticationApiKeyOptionValidator();
        var result = validator.Validate("Default", new AuthenticationApiKeyOption { KeyName = "X-API-KEY", Keys = [] });

        Assert.True(result.Failed);
        Assert.Contains("at least one key", result.FailureMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AK_U_10_RealmKey_secondary_realm_from_config()
    {
        var data = new Dictionary<string, string?>
        {
            ["Authentication:ApiKey:Default:KeyName"] = "X-API-KEY",
            ["Authentication:ApiKey:Default:Mode"] = "RealmKey",
            ["Authentication:ApiKey:Default:Keys:0"] = "default-secret",
            ["Authentication:ApiKey:Integration:KeyName"] = "X-API-KEY",
            ["Authentication:ApiKey:Integration:Mode"] = "RealmKey",
            ["Authentication:ApiKey:Integration:Keys:0"] = "integration-secret",
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(data).Build();

        var services = new ServiceCollection();
        services.AddLogging();
        foreach (var realm in new[] { "Default", "Integration" })
        {
            var section = config.GetSection($"Authentication:ApiKey:{realm}");
            services.Configure<AuthenticationApiKeyOption>(realm, section);
            services.AddOptions<AuthenticationApiKeyOption>(realm).Bind(section);
        }

        services.AddSingleton<IPostConfigureOptions<AuthenticationApiKeyOption>, AuthenticationApiKeyPostConfigureOptions>();
        services.Configure<ApiKeyProviderOptions>(o => o.DefaultSchemeName = "Default");

        var sp = services.BuildServiceProvider();
        var factory = sp.GetRequiredService<IOptionsFactory<AuthenticationApiKeyOption>>();
        var post = sp.GetRequiredService<IPostConfigureOptions<AuthenticationApiKeyOption>>();
        foreach (var realm in new[] { "Default", "Integration" })
            post.PostConfigure(realm, factory.Create(realm));

        var provider = new ApiKeyProvider(
            factory,
            sp.GetRequiredService<IOptions<ApiKeyProviderOptions>>(),
            NullLogger<ApiKeyProvider>.Instance);

        Assert.NotNull(await provider.ProvideAsync("Default:default-secret"));
        Assert.NotNull(await provider.ProvideAsync("Integration:integration-secret"));
        Assert.Null(await provider.ProvideAsync("Integration:wrong"));
    }

    [Fact]
    public void AK_U_09_Missing_key_name_fails_validation()
    {
        var validator = new AuthenticationApiKeyOptionValidator();
        var result = validator.Validate("Default", new AuthenticationApiKeyOption { KeyName = "", Keys = ["x"] });

        Assert.True(result.Failed);
    }
}
