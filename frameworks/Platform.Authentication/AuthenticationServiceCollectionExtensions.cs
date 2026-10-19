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
    /// <para><c>Authentication:Schemes:*:Enabled</c> — host đăng ký <c>AddCore*</c> theo flag (xem Sample).</para>
    /// <para><c>DefaultAuthenticateScheme</c> / <c>DefaultChallengeScheme</c> — một scheme; bỏ trống thì fallback <c>DefaultScheme</c> (map 1:1 <see cref="AuthenticationOptions"/>).</para>
    /// </remarks>
    public static AuthenticationBuilder AddPlatformAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<AuthenticationBuilder>? configureSchemes = null)
    {
        services.TryAddSingleton(configuration);
        var section = configuration.GetSection("Authentication");
        services.Configure<AuthenticationRootOptions>(section);
        services.AddOptions<AuthenticationRootOptions>().Bind(section).ValidateOnStart();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<AuthenticationRootOptions>, AuthenticationRootOptionsValidator>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<AuthenticationRootOptions>, PasswordPolicyRegistrationValidator>());
        services.TryAddSingleton<IPasswordPolicyScopeProvider, EmptyPasswordPolicyScopeProvider>();
        services.TryAddSingleton<IPasswordPolicyProvider, NullPasswordPolicyProvider>();
        services.TryAddSingleton<IPasswordPolicyValidator, PlatformPasswordPolicyValidator>();

        var root = section.Get<AuthenticationRootOptions>() ?? new AuthenticationRootOptions();
        var builder = services.AddAuthentication(options => AuthenticationSchemeResolution.ApplyTo(options, root));
        configureSchemes?.Invoke(builder);
        return builder;
    }
}
