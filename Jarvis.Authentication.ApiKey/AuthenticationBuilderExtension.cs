using AspNetCore.Authentication.ApiKey;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.Authentication.ApiKey;

public static class AuthenticationBuilderExtension
{
    public static AuthenticationBuilder AddCoreApiKey<T>(this AuthenticationBuilder builder, IConfiguration configuration, Action<ApiKeyOptions>? configureOptions = null) where T : class, IApiKeyProvider => builder.AddCoreApiKey<T>(configuration, ApiKeyDefaults.AuthenticationScheme, ApiKeyDefaults.AuthenticationScheme, configureOptions);

    public static AuthenticationBuilder AddCoreApiKey<T>(this AuthenticationBuilder builder, IConfiguration configuration, string authenticationScheme, Action<ApiKeyOptions>? configureOptions = null) where T : class, IApiKeyProvider => builder.AddCoreApiKey<T>(configuration, authenticationScheme, authenticationScheme, configureOptions);

    public static AuthenticationBuilder AddCoreApiKey<T>(this AuthenticationBuilder builder, IConfiguration configuration, string authenticationScheme, string displayName, Action<ApiKeyOptions>? configureOptions = null) where T : class, IApiKeyProvider
    {
        if (configureOptions != null)
            return builder.AddApiKeyInHeader<T>(authenticationScheme, displayName ?? authenticationScheme, configureOptions);

        var section = configuration.GetSection($"Authentication:ApiKey:{authenticationScheme}");
        builder.Services.Configure<AuthenticationApiKeyOption>(authenticationScheme, section);
        var authOption = section.Get<AuthenticationApiKeyOption>();

        return builder.AddApiKeyInHeader<T>(authenticationScheme, displayName ?? authenticationScheme, options =>
        {
            options.Realm = authenticationScheme;
            options.KeyName = authOption?.KeyName;
        });
    }
}