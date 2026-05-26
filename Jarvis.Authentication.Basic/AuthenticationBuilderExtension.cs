using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Jarvis.Authentication.Basic;

/// <summary>
/// Extension đăng ký scheme HTTP Basic Authentication vào ASP.NET Core Authentication pipeline.
/// </summary>
public static class AuthenticationBuilderExtension
{
    /// <summary>
    /// Đăng ký Basic Auth với <see cref="ConfigBasicCredentialValidator"/> mặc định (đọc user/password từ config).
    /// </summary>
    /// <remarks>
    /// <para><b>Khi nào dùng:</b> host chỉ cần user tĩnh trong <c>appsettings</c>, không custom validate.</para>
    /// <para>
    /// <paramref name="schemeName"/> = tên scheme ASP.NET Core (mặc định <c>"Basic"</c>).
    /// <paramref name="configurationKey"/> = key section config (mặc định <c>"Default"</c> → <c>Authentication:Basic:Default</c>).
    /// </para>
    /// </remarks>
    public static AuthenticationBuilder AddCoreBasic(
        this AuthenticationBuilder builder,
        IConfiguration configuration,
        string schemeName = Jarvis.Authentication.JarvisAuthenticationSchemes.Basic,
        string configurationKey = "Default",
        Action<AuthenticationSchemeOptions>? configureOptions = null) =>
        builder.AddCoreBasic<ConfigBasicCredentialValidator>(configuration, schemeName, configurationKey, configureOptions);

    /// <summary>
    /// Đăng ký Basic Auth với delegate tra credential từ nguồn ngoài config (DB, cache, vault, …).
    /// </summary>
    /// <remarks>
    /// <para><b>Chức năng:</b> dùng <see cref="DelegateBasicCredentialValidator"/> — host chỉ cần cung cấp
    /// <paramref name="lookup"/> để lấy <see cref="BasicUserCredential"/> theo username.</para>
    /// <para><b>Khi nào dùng:</b> user/password không nằm trong <c>appsettings</c>; config chỉ giữ <c>Realm</c>
    /// (section <c>Authentication:Basic:{configurationKey}</c>).</para>
    /// <example>
    /// <code>
    /// auth.AddCoreBasic(configuration, async (scheme, username, ct) =>
    /// {
    ///     var user = await db.Users.FindByNameAsync(username, ct);
    ///     return user is null ? null : new BasicUserCredential { Password = user.PasswordHash, Roles = user.Roles };
    /// });
    /// </code>
    /// </example>
    /// </remarks>
    public static AuthenticationBuilder AddCoreBasic(
        this AuthenticationBuilder builder,
        IConfiguration configuration,
        BasicCredentialLookupAsync lookup,
        string schemeName = Jarvis.Authentication.JarvisAuthenticationSchemes.Basic,
        string configurationKey = "Default",
        Action<AuthenticationSchemeOptions>? configureOptions = null) =>
        builder.AddCoreBasic(
            configuration,
            new DelegateBasicCredentialValidator(lookup),
            schemeName,
            configurationKey,
            requireConfigUsers: false,
            configureOptions);

    /// <summary>
    /// Đăng ký Basic Auth với validator tùy chỉnh <typeparamref name="TValidator"/>.
    /// </summary>
    /// <remarks>
    /// <para><b>Khi nào dùng:</b> cần toàn quyền validate (hash password, MFA, …) thay vì lookup + so khớp plain text.</para>
    /// </remarks>
    public static AuthenticationBuilder AddCoreBasic<TValidator>(
        this AuthenticationBuilder builder,
        IConfiguration configuration,
        string schemeName = Jarvis.Authentication.JarvisAuthenticationSchemes.Basic,
        string configurationKey = "Default",
        Action<AuthenticationSchemeOptions>? configureOptions = null)
        where TValidator : class, IBasicCredentialValidator =>
        builder.AddCoreBasic<TValidator>(
            configuration,
            schemeName,
            configurationKey,
            requireConfigUsers: true,
            configureOptions);

    private static AuthenticationBuilder AddCoreBasic<TValidator>(
        this AuthenticationBuilder builder,
        IConfiguration configuration,
        string schemeName,
        string configurationKey,
        bool requireConfigUsers,
        Action<AuthenticationSchemeOptions>? configureOptions)
        where TValidator : class, IBasicCredentialValidator
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<AuthenticationBasicOption>>(
            new AuthenticationBasicOptionValidator(requireConfigUsers)));
        builder.Services.TryAddSingleton<IBasicCredentialValidator, TValidator>();

        return builder.RegisterBasicScheme(configuration, schemeName, configurationKey, configureOptions);
    }

    private static AuthenticationBuilder AddCoreBasic(
        this AuthenticationBuilder builder,
        IConfiguration configuration,
        IBasicCredentialValidator validator,
        string schemeName,
        string configurationKey,
        bool requireConfigUsers,
        Action<AuthenticationSchemeOptions>? configureOptions)
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<AuthenticationBasicOption>>(
            new AuthenticationBasicOptionValidator(requireConfigUsers)));
        builder.Services.AddSingleton<IBasicCredentialValidator>(validator);

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
        builder.Services.AddOptions<AuthenticationBasicOption>(schemeName)
            .Bind(section)
            .ValidateOnStart();

        return builder.AddScheme<AuthenticationSchemeOptions, JarvisBasicAuthenticationHandler>(
            schemeName,
            schemeName,
            _ => { });
    }
}
