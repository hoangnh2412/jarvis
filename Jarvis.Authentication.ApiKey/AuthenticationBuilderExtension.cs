using AspNetCore.Authentication.ApiKey;
using Jarvis.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Jarvis.Authentication.ApiKey;

/// <summary>
/// Extension đăng ký scheme xác thực API Key vào ASP.NET Core Authentication pipeline.
/// </summary>
public static class AuthenticationBuilderExtension
{
    /// <summary>
    /// Đăng ký xác thực API Key với provider <typeparamref name="T"/> (ví dụ <see cref="ConfigApiKeyProvider"/>).
    /// </summary>
    /// <remarks>
    /// <para><see cref="ConfigApiKeyProvider"/> — đọc key từ config; type khác — DB, vault, Redis, MinIO, …</para>
    /// <para>
    /// <paramref name="authenticationScheme"/> phải trùng section <c>Authentication:ApiKey:{scheme}</c>
    /// và tên dùng trong <c>[Authorize(AuthenticationSchemes = "...")]</c>.
    /// </para>
    /// <para>
    /// <typeparamref name="T"/> được đăng ký <b>Singleton</b> (<see cref="IApiKeyProvider"/>).
    /// Không inject scoped <c>DbContext</c> trực tiếp — dùng <c>IDbContextFactory&lt;TContext&gt;</c>
    /// hoặc <c>IServiceScopeFactory</c>. Redis (<c>IConnectionMultiplexer</c> / <c>IDistributedCache</c>)
    /// và MinIO client thường Singleton-safe.
    /// </para>
    /// <para>
    /// Với <see cref="ConfigApiKeyProvider"/>, startup bắt buộc <c>Key</c> trong config.
    /// Custom <typeparamref name="T"/> chỉ bắt buộc <c>KeyName</c> (header) — <c>Key</c> có thể rỗng.
    /// </para>
    /// </remarks>
    /// <remarks>
    /// <para><b>Khi <paramref name="configureOptions"/> là <c>null</c> (mặc định — khuyến nghị):</b></para>
    /// <list type="bullet">
    /// <item>Bind section <c>Authentication:ApiKey:{authenticationScheme}</c> (ví dụ <c>Authentication:ApiKey:Default</c>).</item>
    /// <item>Đăng ký mọi realm con dưới <c>Authentication:ApiKey</c> (multi-realm / <c>realm:secret</c>).</item>
    /// <item>Đặt <c>KeyName</c> header từ config; validate options lúc startup.</item>
    /// </list>
    /// <para><b>Khi truyền <paramref name="configureOptions"/>:</b></para>
    /// <list type="bullet">
    /// <item>Bỏ qua bind <c>appsettings</c> cho <see cref="ApiKeyOptions"/>; cấu hình hoàn toàn bằng delegate.</item>
    /// <item>Dùng cho test, prototype, hoặc khi header name/realm không nằm trong config.</item>
    /// </list>
    /// <para>
    /// Luôn đăng ký <typeparamref name="T"/> vào DI (<see cref="IApiKeyProvider"/>) dạng Singleton.
    /// <paramref name="authenticationScheme"/> phải trùng key section config và tên dùng trong
    /// <c>[Authorize(AuthenticationSchemes = "...")]</c>.
    /// </para>
    /// <example>
    /// <code>
    /// // Config-backed:
    /// auth.AddCoreApiKey&lt;ConfigApiKeyProvider&gt;(config);
    ///
    /// // DB / Redis / MinIO — implement IApiKeyProvider, inject IDbContextFactory hoặc client Singleton:
    /// auth.AddCoreApiKey&lt;MyDbApiKeyProvider&gt;(config);
    ///
    /// // Test / code-first:
    /// auth.AddCoreApiKey&lt;ConfigApiKeyProvider&gt;(config, "Default", "API Key", o => o.KeyName = "X-API-KEY");
    /// </code>
    /// </example>
    /// </remarks>
    public static AuthenticationBuilder AddCoreApiKey<T>(
        this AuthenticationBuilder builder,
        IConfiguration configuration,
        string authenticationScheme = JarvisAuthenticationSchemes.ApiKey,
        string displayName = JarvisAuthenticationSchemes.ApiKey,
        Action<ApiKeyOptions>? configureOptions = null)
        where T : class, IApiKeyProvider
    {
        var requireConfigKey = typeof(T) == typeof(ConfigApiKeyProvider);

        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<AuthenticationApiKeyOption>, AuthenticationApiKeyOptionValidator>());

        // DefaultRealm (không tên) cho provider — fallback khi header không có prefix "realm:".
        builder.Services.Configure<ApiKeyProviderOptions>(o => o.DefaultRealm = authenticationScheme);

        if (configureOptions != null)
        {
            builder.Services.TryAddSingleton<IApiKeyProvider, T>();
            return builder.AddApiKeyInHeader<T>(authenticationScheme, displayName, configureOptions);
        }

        ConfigureApiKeyRealms(builder.Services, configuration, authenticationScheme, requireConfigKey);

        var section = configuration.GetSection($"Authentication:ApiKey:{authenticationScheme}");
        builder.Services.TryAddSingleton<IApiKeyProvider, T>();

        var authOption = section.Get<AuthenticationApiKeyOption>();

        return builder.AddApiKeyInHeader<T>(authenticationScheme, displayName, options =>
        {
            options.Realm = authenticationScheme;
            options.KeyName = authOption?.KeyName;
        });
    }

    /// <summary>
    /// Bind mọi realm con dưới <c>Authentication:ApiKey</c> thành named options.
    /// </summary>
    /// <remarks>
    /// <paramref name="requireConfigKey"/> được gắn theo <b>từng realm</b> (named <see cref="ApiKeyProviderOptions"/>)
    /// để nhiều scheme/provider khác nhau không ghi đè cờ validate của nhau.
    /// </remarks>
    private static void ConfigureApiKeyRealms(
        IServiceCollection services,
        IConfiguration configuration,
        string primaryScheme,
        bool requireConfigKey)
    {
        var apiKeySection = configuration.GetSection("Authentication:ApiKey");
        foreach (var child in apiKeySection.GetChildren())
        {
            var realmName = child.Key;
            var snapshot = child.Get<AuthenticationApiKeyOption>();
            if (realmName != primaryScheme && string.IsNullOrWhiteSpace(snapshot?.Key))
                continue;

            services.Configure<AuthenticationApiKeyOption>(realmName, child);
            services.Configure<ApiKeyProviderOptions>(realmName, o =>
            {
                o.DefaultRealm = primaryScheme;
                o.RequireConfigKey = requireConfigKey;
            });
            services.AddOptions<AuthenticationApiKeyOption>(realmName)
                .Bind(child)
                .ValidateOnStart();
        }
    }
}
