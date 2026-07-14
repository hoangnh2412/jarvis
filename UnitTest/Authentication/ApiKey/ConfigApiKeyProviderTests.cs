using AspNetCore.Authentication.ApiKey;
using Jarvis.Authentication.ApiKey;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using UnitTest.Authentication.Helpers;

namespace UnitTest.Authentication.ApiKey;

/// <summary>Unit test <see cref="ConfigApiKeyProvider"/> — realm mặc định <c>Default</c> và multi-realm.</summary>
public class ConfigApiKeyProviderTests
{
    private static ConfigApiKeyProvider CreateProvider(IConfiguration configuration, string defaultRealm = "Default")
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var section = configuration.GetSection($"Authentication:ApiKey:{defaultRealm}");
        services.Configure<AuthenticationApiKeyOption>(defaultRealm, section);
        services.AddOptions<AuthenticationApiKeyOption>(defaultRealm).Bind(section);
        services.Configure<ApiKeyProviderOptions>(o => o.DefaultRealm = defaultRealm);

        var provider = services.BuildServiceProvider();
        return new ConfigApiKeyProvider(
            provider.GetRequiredService<IOptionsFactory<AuthenticationApiKeyOption>>(),
            provider.GetRequiredService<IOptions<ApiKeyProviderOptions>>(),
            NullLogger<ConfigApiKeyProvider>.Instance);
    }

    private static AuthenticationApiKeyOptionValidator CreateValidator(bool requireConfigKey = true) =>
        new(Options.Create(new ApiKeyProviderOptions { RequireConfigKey = requireConfigKey }));

    /// <summary>Header secret thuần (không prefix) — realm mặc định <c>Default</c>, key hợp lệ.</summary>
    [Fact]
    public async Task AK_U_01_Default_realm_plain_secret_valid()
    {
        var config = AuthenticationConfigurationBuilder.BuildApiKeyConfig(key: "secret");
        var provider = CreateProvider(config);

        var result = await provider.ProvideAsync("secret");

        Assert.NotNull(result);
        Assert.Equal("Default", result!.OwnerName);
    }

    /// <summary>Secret sai — trả về null.</summary>
    [Fact]
    public async Task AK_U_02_Wrong_secret_returns_null()
    {
        var provider = CreateProvider(AuthenticationConfigurationBuilder.BuildApiKeyConfig(key: "secret"));

        Assert.Null(await provider.ProvideAsync("wrong"));
    }

    /// <summary>Header dạng <c>Default:secret</c> — vẫn hợp lệ khi key khớp.</summary>
    [Fact]
    public async Task AK_U_03_Default_realm_explicit_prefix_valid()
    {
        var provider = CreateProvider(AuthenticationConfigurationBuilder.BuildApiKeyConfig(key: "secret"));

        Assert.NotNull(await provider.ProvideAsync("Default:secret"));
    }

    /// <summary>Prefix realm rõ ràng <c>Default:secret</c>.</summary>
    [Fact]
    public async Task AK_U_04_Explicit_realm_valid()
    {
        var config = AuthenticationConfigurationBuilder.BuildApiKeyConfig(key: "s1");
        var provider = CreateProvider(config);

        Assert.NotNull(await provider.ProvideAsync("Default:s1"));
    }

    /// <summary>Realm <c>Other</c> không có trong config — từ chối.</summary>
    [Fact]
    public async Task AK_U_06_Unknown_realm_returns_null()
    {
        var provider = CreateProvider(AuthenticationConfigurationBuilder.BuildApiKeyConfig(key: "s1"));

        Assert.Null(await provider.ProvideAsync("Other:s1"));
    }

    /// <summary><c>Key</c> rỗng + <see cref="ApiKeyProviderOptions.RequireConfigKey"/> — validator fail.</summary>
    [Fact]
    public void AK_U_08_Empty_key_fails_validation()
    {
        var validator = CreateValidator(requireConfigKey: true);
        var result = validator.Validate("Default", new AuthenticationApiKeyOption { KeyName = "X-API-KEY", Key = "" });

        Assert.True(result.Failed);
        Assert.Contains("Key is required", result.FailureMessage, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Custom provider — <c>RequireConfigKey=false</c> cho phép <c>Key</c> rỗng.</summary>
    [Fact]
    public void AK_U_08b_Empty_key_ok_when_require_config_key_false()
    {
        var validator = CreateValidator(requireConfigKey: false);
        var result = validator.Validate("Default", new AuthenticationApiKeyOption { KeyName = "X-API-KEY", Key = "" });

        Assert.False(result.Failed);
    }

    /// <summary>Hai realm <c>Default</c> và <c>Integration</c> — mỗi realm một secret riêng.</summary>
    [Fact]
    public async Task AK_U_10_Secondary_realm_from_config()
    {
        var data = new Dictionary<string, string?>
        {
            ["Authentication:ApiKey:Default:KeyName"] = "X-API-KEY",
            ["Authentication:ApiKey:Default:Key"] = "default-secret",
            ["Authentication:ApiKey:Integration:KeyName"] = "X-API-KEY",
            ["Authentication:ApiKey:Integration:Key"] = "integration-secret",
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

        services.Configure<ApiKeyProviderOptions>(o => o.DefaultRealm = "Default");

        var sp = services.BuildServiceProvider();
        var provider = new ConfigApiKeyProvider(
            sp.GetRequiredService<IOptionsFactory<AuthenticationApiKeyOption>>(),
            sp.GetRequiredService<IOptions<ApiKeyProviderOptions>>(),
            NullLogger<ConfigApiKeyProvider>.Instance);

        Assert.NotNull(await provider.ProvideAsync("default-secret"));
        Assert.NotNull(await provider.ProvideAsync("Default:default-secret"));
        Assert.NotNull(await provider.ProvideAsync("Integration:integration-secret"));
        Assert.Null(await provider.ProvideAsync("Integration:wrong"));
    }

    /// <summary>Thiếu <c>KeyName</c> — validator startup fail.</summary>
    [Fact]
    public void AK_U_09_Missing_key_name_fails_validation()
    {
        var validator = CreateValidator();
        var result = validator.Validate("Default", new AuthenticationApiKeyOption { KeyName = "", Key = "x" });

        Assert.True(result.Failed);
    }

    /// <summary><see cref="ConfigApiKeyProvider"/> + <c>Key</c> rỗng → <c>ProvideAsync</c> trả null.</summary>
    [Fact]
    public async Task AK_U_11_Empty_config_key_provide_returns_null()
    {
        var provider = CreateProvider(AuthenticationConfigurationBuilder.BuildApiKeyConfig(key: ""));

        Assert.Null(await provider.ProvideAsync("anything"));
    }
}
