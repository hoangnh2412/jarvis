using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Platform.Authentication;

namespace Platform.Authentication.Jwt;

/// <summary>
/// Extension đăng ký scheme JWT Bearer vào ASP.NET Core Authentication pipeline.
/// </summary>
public static class AuthenticationBuilderExtension
{
    /// <summary>
    /// Đăng ký JwtBearer scheme <see cref="PlatformAuthenticationSchemes.Bearer"/>, bind <c>Authentication:Schemes:Bearer:Profiles:Default</c>.
    /// </summary>
    public static AuthenticationBuilder AddCoreJwtBearer(
        this AuthenticationBuilder builder,
        IConfiguration configuration,
        Action<JwtBearerOptions>? configureOptions = null) =>
        builder.AddCoreJwtBearer(
            configuration,
            PlatformAuthenticationSchemes.Bearer,
            PlatformAuthenticationSchemes.DefaultProfile,
            configureOptions);

    /// <summary>
    /// Đăng ký JwtBearer — bind <c>Authentication:Schemes:Bearer:Profiles:{configurationProfile}</c>.
    /// </summary>
    public static AuthenticationBuilder AddCoreJwtBearer(
        this AuthenticationBuilder builder,
        IConfiguration configuration,
        string authenticationScheme,
        string configurationProfile,
        Action<JwtBearerOptions>? configureOptions = null) =>
        builder.AddCoreJwtBearer(
            configuration,
            authenticationScheme,
            authenticationScheme,
            configurationProfile,
            configureOptions);

    /// <summary>
    /// Overload đầy đủ: đăng ký JwtBearer, bind config và map sang <see cref="JwtBearerOptions"/>.
    /// </summary>
    public static AuthenticationBuilder AddCoreJwtBearer(
        this AuthenticationBuilder builder,
        IConfiguration configuration,
        string authenticationScheme,
        string displayName,
        string configurationProfile,
        Action<JwtBearerOptions>? configureOptions = null)
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<AuthenticationJwtOption>, AuthenticationJwtOptionValidator>());

        if (configureOptions != null)
            return builder.AddJwtBearer(authenticationScheme, displayName ?? authenticationScheme, configureOptions);

        var section = configuration.GetSection(AuthenticationSchemeProfileConfiguration.ProfileSection("Bearer", configurationProfile));
        builder.Services.Configure<AuthenticationJwtOption>(authenticationScheme, section);
        builder.Services.AddOptions<AuthenticationJwtOption>(authenticationScheme)
            .Bind(section)
            .ValidateOnStart();

        var authOption = section.Get<AuthenticationJwtOption>() ?? new AuthenticationJwtOption();

        return builder.AddJwtBearer(authenticationScheme, displayName ?? authenticationScheme, options =>
        {
            ConfigureJwtBearer(options, authOption, builder.Services);
        });
    }

    /// <summary>
    /// Map <see cref="AuthenticationJwtOption"/> sang <see cref="JwtBearerOptions"/> và <see cref="TokenValidationParameters"/>.
    /// </summary>
    public static void ConfigureJwtBearer(JwtBearerOptions options, AuthenticationJwtOption authOption, IServiceCollection _)
    {
        options.RequireHttpsMetadata = authOption.RequireHttpsMetadata ?? true;
        options.SaveToken = true;

        var useAuthority = !string.IsNullOrWhiteSpace(authOption.Authority);
        if (useAuthority)
        {
            options.Authority = authOption.Authority!.TrimEnd('/');
            if (!string.IsNullOrWhiteSpace(authOption.Audience))
                options.Audience = authOption.Audience;
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ClockSkew = TimeSpan.Zero,
            ValidateActor = authOption.ValidateActor,
            ValidateSignatureLast = authOption.ValidateSignatureLast,
            ValidateWithLKG = authOption.ValidateWithLKG,
            ValidateTokenReplay = authOption.ValidateTokenReplay,
            ValidateAudience = authOption.ValidateAudience,
            ValidAudiences = ResolveValidAudiences(authOption),
            ValidateIssuerSigningKey = useAuthority ? false : authOption.ValidateIssuerSigningKey,
            IssuerSigningKeys = useAuthority
                ? null
                : authOption.IssuerSigningKeys.Select(x => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(x))),
            ValidateIssuer = useAuthority || authOption.ValidateIssuer,
            ValidIssuers = authOption.ValidIssuers.Length > 0 ? authOption.ValidIssuers : null,
            ValidateLifetime = true,
            LifetimeValidator = authOption.MaxExpireMinutes > 0
                ? (notBefore, expires, _, parameters) => ValidateMaxLifetime(notBefore, expires, parameters, authOption.MaxExpireMinutes)
                : null
        };
    }

    private static string[]? ResolveValidAudiences(AuthenticationJwtOption authOption)
    {
        if (authOption.ValidAudiences.Length > 0)
            return authOption.ValidAudiences;

        return string.IsNullOrWhiteSpace(authOption.Audience) ? null : [authOption.Audience];
    }

    /// <summary>Giới hạn thời gian sống token tối đa theo <see cref="AuthenticationJwtOption.MaxExpireMinutes"/>.</summary>
    private static bool ValidateMaxLifetime(
        DateTime? notBefore,
        DateTime? expires,
        TokenValidationParameters validationParameters,
        int maxExpireMinutes)
    {
        if (!validationParameters.ValidateLifetime)
            return true;

        if (notBefore.HasValue && expires.HasValue
            && (expires.Value - notBefore.Value).TotalMinutes > maxExpireMinutes)
        {
            throw LogHelper.LogExceptionMessage(
                new SecurityTokenInvalidLifetimeException($"Token lifetime exceeds {maxExpireMinutes} minutes.")
                {
                    NotBefore = notBefore,
                    Expires = expires
                });
        }

        return true;
    }
}
