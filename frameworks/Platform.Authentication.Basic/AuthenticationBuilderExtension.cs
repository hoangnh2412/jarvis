using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Platform.Authentication;

namespace Platform.Authentication.Basic;

/// <summary>
/// Extension đăng ký scheme HTTP Basic Authentication vào ASP.NET Core Authentication pipeline.
/// </summary>
public static class AuthenticationBuilderExtension
{
    /// <summary>
    /// Đăng ký Basic Auth với <typeparamref name="TCredentialProvider"/> qua DI.
    /// </summary>
    /// <remarks>
    /// <para><b>Chức năng:</b> bind scheme + đăng ký <see cref="IBasicCredentialProvider"/> — host chọn nguồn credential (config, DB, …).</para>
    /// <para><see cref="ConfigBasicCredentialProvider"/> — đọc từ <c>appsettings</c>; type khác — DB, API, …</para>
    /// </remarks>
    public static AuthenticationBuilder AddCoreBasic<TCredentialProvider>(
        this AuthenticationBuilder builder,
        IConfiguration configuration,
        string schemeName = AuthenticationBasicOption.DefaultScheme,
        string configurationKey = AuthenticationBasicOption.DefaultRealm,
        Action<AuthenticationSchemeOptions>? configureOptions = null)
        where TCredentialProvider : class, IBasicCredentialProvider
    {
        builder.Services.TryAddSingleton<IBasicCredentialProvider, TCredentialProvider>();

        return builder.RegisterBasicScheme(configuration, schemeName, configurationKey, configureOptions);
    }

    /// <summary>Basic — profile active (<c>Schemes.Basic.Profile</c>) chọn <see cref="AuthenticationBasicOption.Source"/>.</summary>
    public static AuthenticationBuilder AddCoreBasicWithProfileSources<TStorage>(
        this AuthenticationBuilder builder,
        IConfiguration configuration,
        string schemeName = AuthenticationBasicOption.DefaultScheme,
        string configurationKey = AuthenticationBasicOption.DefaultRealm)
        where TStorage : class, IBasicCredentialProvider
    {
        builder.Services.TryAddSingleton<ConfigBasicCredentialProvider>();
        builder.Services.TryAddSingleton<TStorage>();
        builder.Services.TryAddSingleton<ProfileSourcedBasicCredentialProvider<TStorage>>();
        builder.Services.TryAddSingleton<IBasicCredentialProvider>(sp =>
            sp.GetRequiredService<ProfileSourcedBasicCredentialProvider<TStorage>>());

        return builder.RegisterBasicScheme(configuration, schemeName, configurationKey, null);
    }

    private static AuthenticationBuilder RegisterBasicScheme(
        this AuthenticationBuilder builder,
        IConfiguration configuration,
        string schemeName,
        string configurationKey,
        Action<AuthenticationSchemeOptions>? configureOptions)
    {
        if (configureOptions != null)
            return builder.AddScheme<AuthenticationSchemeOptions, PlatformBasicAuthenticationHandler>(
                schemeName,
                schemeName,
                configureOptions);

        ConfigureBasicProfiles(builder.Services, configuration);

        var section = configuration.GetSection(AuthenticationSchemeProfileConfiguration.ProfileSection("Basic", configurationKey));
        builder.Services.Configure<AuthenticationBasicOption>(schemeName, section);
        builder.Services.AddOptions<AuthenticationBasicOption>(schemeName).Bind(section);

        return builder.AddScheme<AuthenticationSchemeOptions, PlatformBasicAuthenticationHandler>(
            schemeName,
            schemeName,
            _ => { });
    }

    private static void ConfigureBasicProfiles(IServiceCollection services, IConfiguration configuration)
    {
        var basicSection = configuration.GetSection(AuthenticationSchemeProfileConfiguration.ProfilesSection("Basic"));
        foreach (var child in basicSection.GetChildren())
        {
            services.Configure<AuthenticationBasicOption>(child.Key, child);
            services.AddOptions<AuthenticationBasicOption>(child.Key).Bind(child);
        }
    }
}
