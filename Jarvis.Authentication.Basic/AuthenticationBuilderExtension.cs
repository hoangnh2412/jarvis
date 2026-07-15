using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Jarvis.Authentication.Basic;

/// <summary>
/// Extension đăng ký scheme HTTP Basic Authentication vào ASP.NET Core Authentication pipeline.
/// </summary>
public static class AuthenticationBuilderExtension
{
    /// <summary>
    /// Đăng ký Basic Auth với <typeparamref name="TCredentialProvider"/> qua DI.
    /// </summary>
    /// <remarks>
    /// <para><b>Chức năng:</b> bind scheme + đăng ký <see cref="IBasicCredentialProvider"/> — host chọn nguồn credential (config, DB, Redis, MinIO, …).</para>
    /// <para><see cref="ConfigBasicCredentialProvider"/> — đọc từ <c>appsettings</c>; type khác — DB, API, …</para>
    /// <para>
    /// <typeparamref name="TCredentialProvider"/> được đăng ký <b>Singleton</b>.
    /// Không inject scoped <c>DbContext</c> trực tiếp — dùng <c>IDbContextFactory&lt;TContext&gt;</c>
    /// hoặc <c>IServiceScopeFactory</c>. Redis / MinIO client thường Singleton-safe.
    /// </para>
    /// <example>
    /// <code>
    /// auth.AddCoreBasic&lt;ConfigBasicCredentialProvider&gt;(config);
    /// auth.AddCoreBasic&lt;MyDbBasicCredentialProvider&gt;(config);
    /// </code>
    /// </example>
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

    private static AuthenticationBuilder RegisterBasicScheme(
        this AuthenticationBuilder builder,
        IConfiguration configuration,
        string schemeName,
        string configurationKey,
        Action<AuthenticationSchemeOptions>? configureOptions)
    {
        if (configureOptions != null)
            return builder.AddScheme<AuthenticationSchemeOptions, JarvisBasicAuthenticationHandler>(
                schemeName,
                schemeName,
                configureOptions);

        var section = configuration.GetSection($"Authentication:Basic:{configurationKey}");
        builder.Services.Configure<AuthenticationBasicOption>(schemeName, section);
        builder.Services.AddOptions<AuthenticationBasicOption>(schemeName).Bind(section);

        return builder.AddScheme<AuthenticationSchemeOptions, JarvisBasicAuthenticationHandler>(
            schemeName,
            schemeName,
            _ => { });
    }
}
