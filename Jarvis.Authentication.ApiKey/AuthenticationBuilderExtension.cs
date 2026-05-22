using AspNetCore.Authentication.ApiKey;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Jarvis.Authentication.ApiKey;

public static class AuthenticationBuilderExtension
{
    public static AuthenticationBuilder AddCoreApiKey(
        this AuthenticationBuilder builder,
        IConfiguration configuration,
        string schemeName = Jarvis.Authentication.JarvisAuthenticationSchemes.ApiKey,
        Action<ApiKeyOptions>? configureOptions = null) =>
        builder.AddCoreApiKey<ApiKeyProvider>(configuration, schemeName, configureOptions);

    public static AuthenticationBuilder AddCoreApiKey<T>(
        this AuthenticationBuilder builder,
        IConfiguration configuration,
        string schemeName = Jarvis.Authentication.JarvisAuthenticationSchemes.ApiKey,
        Action<ApiKeyOptions>? configureOptions = null)
        where T : class, IApiKeyProvider =>
        builder.AddCoreApiKey<T>(configuration, schemeName, schemeName, configureOptions);

    public static AuthenticationBuilder AddCoreApiKey<T>(
        this AuthenticationBuilder builder,
        IConfiguration configuration,
        string authenticationScheme,
        string displayName,
        Action<ApiKeyOptions>? configureOptions = null)
        where T : class, IApiKeyProvider
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<AuthenticationApiKeyOption>, AuthenticationApiKeyPostConfigureOptions>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<AuthenticationApiKeyOption>, AuthenticationApiKeyOptionValidator>());

        if (configureOptions != null)
        {
            builder.Services.TryAddSingleton<IApiKeyProvider, T>();
            return builder.AddApiKeyInHeader<T>(authenticationScheme, displayName, configureOptions);
        }

        ConfigureApiKeyRealms(builder.Services, configuration, authenticationScheme);

        builder.Services.Configure<ApiKeyProviderOptions>(o => o.DefaultSchemeName = authenticationScheme);

        var section = configuration.GetSection($"Authentication:ApiKey:{authenticationScheme}");
        builder.Services.TryAddSingleton<IApiKeyProvider, T>();

        var authOption = section.Get<AuthenticationApiKeyOption>();

        return builder.AddApiKeyInHeader<T>(authenticationScheme, displayName, options =>
        {
            options.Realm = authenticationScheme;
            options.KeyName = authOption?.KeyName;
        });
    }

    private static void ConfigureApiKeyRealms(
        IServiceCollection services,
        IConfiguration configuration,
        string primaryScheme)
    {
        var apiKeySection = configuration.GetSection("Authentication:ApiKey");
        foreach (var child in apiKeySection.GetChildren())
        {
            var realmName = child.Key;
            var snapshot = child.Get<AuthenticationApiKeyOption>();
            if (realmName != primaryScheme && snapshot?.Keys.Length == 0)
                continue;

            services.Configure<AuthenticationApiKeyOption>(realmName, child);
            services.AddOptions<AuthenticationApiKeyOption>(realmName)
                .Bind(child)
                .ValidateOnStart();
        }
    }
}
