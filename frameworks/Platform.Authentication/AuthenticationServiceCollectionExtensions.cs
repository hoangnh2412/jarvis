using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Platform.Authentication;

/// <summary>
/// Entry point đăng ký authentication Platform — bind root config và gọi satellite scheme (Jwt, ApiKey, Basic).
/// </summary>
public static class AuthenticationServiceCollectionExtensions
{
    /// <summary>
    /// Khởi tạo ASP.NET Core Authentication với <see cref="AuthenticationRootOptions"/> từ <c>Authentication</c> section.
    /// </summary>
    /// <remarks>
    /// <para><b>Chức năng:</b></para>
    /// <list type="bullet">
    /// <item>Bind <c>Authentication</c> → <see cref="AuthenticationRootOptions"/> và validate startup.</item>
    /// <item>Đặt default authenticate/challenge scheme từ config.</item>
    /// <item>Đăng ký <see cref="IPasswordPolicyValidator"/> mặc định.</item>
    /// <item>Gọi <paramref name="configureSchemes"/> để satellite packages thêm scheme (Jwt, ApiKey, Basic).</item>
    /// </list>
    /// <para><b>Khi nào dùng:</b> luôn gọi đầu tiên trước <c>AddCoreJwtBearer</c> / <c>AddCoreApiKey</c> / <c>AddCoreBasic</c>.
    /// Host không gọi trực tiếp <c>services.AddAuthentication()</c> nếu dùng Platform auth stack.</para>
    /// <example>
    /// <code>
    /// builder.Services.AddPlatformAuthentication(configuration, auth =>
    /// {
    ///     auth.AddCoreJwtBearer(configuration, "Bearer");
    ///     auth.AddCoreApiKey(configuration, PlatformAuthenticationSchemes.ApiKey);
    /// });
    /// </code>
    /// </example>
    /// </remarks>
    public static AuthenticationBuilder AddPlatformAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<AuthenticationBuilder>? configureSchemes = null)
    {
        var section = configuration.GetSection("Authentication");
        services.Configure<AuthenticationRootOptions>(section);
        services.AddOptions<AuthenticationRootOptions>()
            .Bind(section)
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<AuthenticationRootOptions>, AuthenticationRootOptionsValidator>();
        services.TryAddSingleton<IPasswordPolicyValidator, DefaultPasswordPolicyValidator>();

        var root = section.Get<AuthenticationRootOptions>() ?? new AuthenticationRootOptions();

        var builder = services.AddAuthentication(options =>
        {
            if (!string.IsNullOrWhiteSpace(root.DefaultAuthenticateScheme))
                options.DefaultAuthenticateScheme = root.DefaultAuthenticateScheme;

            if (!string.IsNullOrWhiteSpace(root.DefaultChallengeScheme))
                options.DefaultChallengeScheme = root.DefaultChallengeScheme;
        });

        configureSchemes?.Invoke(builder);
        return builder;
    }
}
