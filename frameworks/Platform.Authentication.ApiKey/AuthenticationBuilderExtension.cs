using AspNetCore.Authentication.ApiKey;
using Platform.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Platform.Authentication.ApiKey;

/// <summary>
/// Extension đăng ký scheme xác thực API Key vào ASP.NET Core Authentication pipeline.
/// </summary>
public static class AuthenticationBuilderExtension
{
    /// <summary>
    /// Đăng ký xác thực API Key với provider <typeparamref name="TProvider"/> (ví dụ <see cref="ConfigApiKeyProvider"/>).
    /// </summary>
    /// <remarks>
    /// <para>Scheme ASP.NET Core: <see cref="PlatformAuthenticationSchemes.ApiKey"/>.</para>
    /// <para><paramref name="profile"/> — profile active (<c>Schemes:ApiKey:Profile</c>); bind <c>Profiles:{profile}</c>, realm mặc định khi header không có <c>realm:</c>.</para>
    /// </remarks>
    public static AuthenticationBuilder AddCoreApiKey<TProvider>(
        this AuthenticationBuilder builder,
        IConfiguration configuration,
        string? profile = null,
        string authenticationScheme = PlatformAuthenticationSchemes.ApiKey,
        string displayName = "API Key",
        Action<ApiKeyOptions>? configureOptions = null)
        where TProvider : class, IApiKeyProvider
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<AuthenticationApiKeyOption>, AuthenticationApiKeyOptionValidator>());

        if (configureOptions != null)
        {
            builder.Services.TryAddSingleton<IApiKeyProvider, TProvider>();
            return builder.AddApiKeyInHeader<TProvider>(authenticationScheme, displayName, configureOptions);
        }

        var activeProfile = ResolveActiveApiKeyProfile(configuration, profile);
        ConfigureApiKeyRealms(builder.Services, configuration);
        builder.Services.Configure<ApiKeyProviderOptions>(o => o.DefaultRealm = activeProfile);

        var section = configuration.GetSection(AuthenticationSchemeProfileConfiguration.ProfileSection("ApiKey", activeProfile));
        builder.Services.TryAddSingleton<IApiKeyProvider, TProvider>();

        var authOption = section.Get<AuthenticationApiKeyOption>();

        return builder.AddApiKeyInHeader<TProvider>(authenticationScheme, displayName, options =>
        {
            options.Realm = activeProfile;
            options.KeyName = authOption?.KeyName;
        });
    }

    /// <summary>
    /// API Key — mỗi profile có <see cref="AuthenticationApiKeyOption.Source"/> riêng (config vs <typeparamref name="TStorage"/>).
    /// </summary>
    public static AuthenticationBuilder AddCoreApiKeyWithProfileSources<TStorage>(
        this AuthenticationBuilder builder,
        IConfiguration configuration,
        string? profile = null,
        string authenticationScheme = PlatformAuthenticationSchemes.ApiKey,
        string displayName = "API Key")
        where TStorage : class, IApiKeyProvider
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<AuthenticationApiKeyOption>, AuthenticationApiKeyOptionValidator>());
        var activeProfile = ResolveActiveApiKeyProfile(configuration, profile);
        ConfigureApiKeyRealms(builder.Services, configuration);
        builder.Services.Configure<ApiKeyProviderOptions>(o => o.DefaultRealm = activeProfile);

        builder.Services.TryAddSingleton<ConfigApiKeyProvider>();
        builder.Services.TryAddSingleton<TStorage>();
        builder.Services.TryAddSingleton<ProfileSourcedApiKeyProvider<TStorage>>();
        builder.Services.TryAddSingleton<IApiKeyProvider>(sp => sp.GetRequiredService<ProfileSourcedApiKeyProvider<TStorage>>());

        var section = configuration.GetSection(AuthenticationSchemeProfileConfiguration.ProfileSection("ApiKey", activeProfile));
        var authOption = section.Get<AuthenticationApiKeyOption>();

        return builder.AddApiKeyInHeader<ProfileSourcedApiKeyProvider<TStorage>>(authenticationScheme, displayName, options =>
        {
            options.Realm = activeProfile;
            options.KeyName = authOption?.KeyName;
        });
    }

    private static string ResolveActiveApiKeyProfile(IConfiguration configuration, string? profile)
    {
        if (!string.IsNullOrWhiteSpace(profile))
            return profile.Trim();

        var root = configuration.GetSection("Authentication").Get<AuthenticationRootOptions>() ?? new AuthenticationRootOptions();
        return AuthenticationSchemeProfileConfiguration.ResolveApiKeyProfile(root, configuration);
    }

    /// <summary>
    /// Bind mọi profile dưới <c>Authentication:Schemes:ApiKey:Profiles</c> thành named options.
    /// </summary>
    private static void ConfigureApiKeyRealms(IServiceCollection services, IConfiguration configuration)
    {
        var apiKeySection = configuration.GetSection(AuthenticationSchemeProfileConfiguration.ProfilesSection("ApiKey"));
        foreach (var child in apiKeySection.GetChildren())
        {
            var realmName = child.Key;
            var snapshot = child.Get<AuthenticationApiKeyOption>();
            if (string.IsNullOrWhiteSpace(snapshot?.KeyName))
                continue;

            services.Configure<AuthenticationApiKeyOption>(realmName, child);
            services.AddOptions<AuthenticationApiKeyOption>(realmName)
                .Bind(child)
                .ValidateOnStart();
        }
    }
}
