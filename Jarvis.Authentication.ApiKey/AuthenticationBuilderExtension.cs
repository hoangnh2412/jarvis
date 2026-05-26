using AspNetCore.Authentication.ApiKey;
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
    /// Đăng ký xác thực API Key với <see cref="ApiKeyProvider"/> mặc định (đọc keys từ config).
    /// </summary>
    /// <remarks>
    /// <para>Dùng khi host không cần custom logic validate key — chỉ cần bind <c>appsettings</c>.</para>
    /// <para>
    /// <c>schemeName</c> mặc định là <see cref="JarvisAuthenticationSchemes.ApiKey"/> (<c>"Default"</c>),
    /// khớp section <c>Authentication:ApiKey:Default</c>.
    /// </para>
    /// <example>
    /// <code>
    /// auth.AddCoreApiKey(builder.Configuration);
    /// // hoặc chỉ định scheme trùng tên section config:
    /// auth.AddCoreApiKey(builder.Configuration, JarvisAuthenticationSchemes.ApiKey);
    /// </code>
    /// </example>
    /// </remarks>
    public static AuthenticationBuilder AddCoreApiKey(
        this AuthenticationBuilder builder,
        IConfiguration configuration,
        string schemeName = JarvisAuthenticationSchemes.ApiKey,
        Action<ApiKeyOptions>? configureOptions = null) =>
        builder.AddCoreApiKey<ApiKeyProvider>(configuration, schemeName, configureOptions);

    /// <summary>
    /// Đăng ký xác thực API Key với provider tùy chỉnh <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Dùng khi cần override cách resolve/validate key (ví dụ đọc từ DB, vault, cache)
    /// thay vì <see cref="ApiKeyProvider"/> built-in.
    /// </para>
    /// <para>
    /// <c>displayName</c> được đặt bằng <paramref name="schemeName"/> (dùng cho UI/challenge).
    /// Cần tách tên hiển thị → gọi overload 3 tham số scheme.
    /// </para>
    /// <example>
    /// <code>
    /// auth.AddCoreApiKey&lt;MyApiKeyProvider&gt;(builder.Configuration, JarvisAuthenticationSchemes.ApiKey);
    /// </code>
    /// </example>
    /// </remarks>
    public static AuthenticationBuilder AddCoreApiKey<T>(
        this AuthenticationBuilder builder,
        IConfiguration configuration,
        string schemeName = JarvisAuthenticationSchemes.ApiKey,
        Action<ApiKeyOptions>? configureOptions = null)
        where T : class, IApiKeyProvider =>
        builder.AddCoreApiKey<T>(configuration, schemeName, schemeName, configureOptions);

    /// <summary>
    /// Overload đầy đủ: đăng ký scheme API Key, bind config và (tuỳ chọn) cấu hình header bằng code.
    /// </summary>
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
    /// Luôn đăng ký <typeparamref name="T"/> vào DI (<see cref="IApiKeyProvider"/>).
    /// <paramref name="authenticationScheme"/> phải trùng key section config và tên dùng trong
    /// <c>[Authorize(AuthenticationSchemes = "...")]</c>.
    /// </para>
    /// <example>
    /// <code>
    /// // Production — bind appsettings:
    /// auth.AddCoreApiKey&lt;ApiKeyProvider&gt;(config, "Default", "API Key");
    ///
    /// // Test / code-first:
    /// auth.AddCoreApiKey&lt;ApiKeyProvider&gt;(config, "Default", "API Key", o => o.KeyName = "X-API-KEY");
    /// </code>
    /// </example>
    /// </remarks>
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

    /// <summary>
    /// Bind mọi realm con dưới <c>Authentication:ApiKey</c> thành named options.
    /// </summary>
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
