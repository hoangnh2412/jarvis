using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Jarvis.Authentication.Basic;

public static class AuthenticationBuilderExtension
{
    public static AuthenticationBuilder AddCoreBasic(
        this AuthenticationBuilder builder,
        IConfiguration configuration,
        string schemeName = Jarvis.Authentication.JarvisAuthenticationSchemes.Basic,
        string configurationKey = "Default",
        Action<AuthenticationSchemeOptions>? configureOptions = null) =>
        builder.AddCoreBasic<ConfigBasicCredentialValidator>(configuration, schemeName, configurationKey, configureOptions);

    public static AuthenticationBuilder AddCoreBasic<TValidator>(
        this AuthenticationBuilder builder,
        IConfiguration configuration,
        string schemeName = Jarvis.Authentication.JarvisAuthenticationSchemes.Basic,
        string configurationKey = "Default",
        Action<AuthenticationSchemeOptions>? configureOptions = null)
        where TValidator : class, IBasicCredentialValidator
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<AuthenticationBasicOption>, AuthenticationBasicOptionValidator>());
        builder.Services.TryAddSingleton<IBasicCredentialValidator, TValidator>();

        if (configureOptions != null)
            return builder.AddScheme<AuthenticationSchemeOptions, JarvisBasicAuthenticationHandler>(
                schemeName,
                schemeName,
                configureOptions);

        var section = configuration.GetSection($"Authentication:Basic:{configurationKey}");
        builder.Services.Configure<AuthenticationBasicOption>(schemeName, section);
        builder.Services.AddOptions<AuthenticationBasicOption>(schemeName)
            .Bind(section)
            .ValidateOnStart();

        return builder.AddScheme<AuthenticationSchemeOptions, JarvisBasicAuthenticationHandler>(
            schemeName,
            schemeName,
            _ => { });
    }
}
