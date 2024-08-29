using AspNetCore.Authentication.ApiKey;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;

namespace Jarvis.Authentication.ApiKey;

public static class AuthenticationBuilderExtension
{
    public static AuthenticationBuilder AddCoreApiKey<T>(this AuthenticationBuilder builder, IConfiguration configuration, Action<ApiKeyOptions>? configureOptions = null) where T : class, IApiKeyProvider => builder.AddCoreApiKey<T>(configuration, ApiKeyDefaults.AuthenticationScheme, ApiKeyDefaults.AuthenticationScheme, configureOptions);

    public static AuthenticationBuilder AddCoreApiKey<T>(this AuthenticationBuilder builder, IConfiguration configuration, string authenticationScheme, Action<ApiKeyOptions>? configureOptions = null) where T : class, IApiKeyProvider => builder.AddCoreApiKey<T>(configuration, authenticationScheme, authenticationScheme, configureOptions);

    public static AuthenticationBuilder AddCoreApiKey<T>(this AuthenticationBuilder builder, IConfiguration configuration, string authenticationScheme, string displayName, Action<ApiKeyOptions>? configureOptions = null) where T : class, IApiKeyProvider
    {
        if (configureOptions != null)
            return builder.AddApiKeyInHeader<T>(authenticationScheme, displayName ?? authenticationScheme, configureOptions);

        var authOption = configuration.GetSection($"Authentication:ApiKey:{authenticationScheme}").Get<AuthenticationApiKeyOption>();

        return builder.AddApiKeyInHeader<T>(authenticationScheme, displayName ?? authenticationScheme, options =>
        {
            options.Realm = authOption?.Realm;
            options.KeyName = authOption?.KeyName;
        });
    }
}