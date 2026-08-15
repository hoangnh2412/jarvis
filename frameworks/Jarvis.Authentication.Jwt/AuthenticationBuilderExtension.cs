using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Jarvis.Authentication.Jwt;

/// <summary>
/// Extension đăng ký scheme JWT Bearer vào ASP.NET Core Authentication pipeline.
/// </summary>
public static class AuthenticationBuilderExtension
{
    /// <summary>
    /// Đăng ký JwtBearer với scheme mặc định <c>Bearer</c>, bind config từ <c>Authentication:Jwt:Bearer</c>.
    /// </summary>
    /// <remarks>
    /// <para><b>Khi nào dùng:</b> API nhận token OAuth/JWT qua header <c>Authorization: Bearer</c>, một scheme duy nhất.</para>
    /// </remarks>
    public static AuthenticationBuilder AddCoreJwtBearer(
        this AuthenticationBuilder builder,
        IConfiguration configuration,
        Action<JwtBearerOptions>? configureOptions = null) =>
        builder.AddCoreJwtBearer(configuration, JwtBearerDefaults.AuthenticationScheme, configureOptions);

    /// <summary>
    /// Đăng ký JwtBearer với tên scheme tùy chỉnh — bind <c>Authentication:Jwt:{authenticationScheme}</c>.
    /// </summary>
    /// <remarks>
    /// <para><b>Khi nào dùng:</b> nhiều JWT authority hoặc scheme name khác <c>Bearer</c> (ví dụ validate token từ OpenIddict nội bộ).</para>
    /// </remarks>
    public static AuthenticationBuilder AddCoreJwtBearer(
        this AuthenticationBuilder builder,
        IConfiguration configuration,
        string authenticationScheme,
        Action<JwtBearerOptions>? configureOptions = null) =>
        builder.AddCoreJwtBearer(configuration, authenticationScheme, authenticationScheme, configureOptions);

    /// <summary>
    /// Overload đầy đủ: đăng ký JwtBearer, bind config và map sang <see cref="JwtBearerOptions"/>.
    /// </summary>
    /// <remarks>
    /// <para><b>Khi <paramref name="configureOptions"/> là <c>null</c> (khuyến nghị):</b></para>
    /// <list type="bullet">
    /// <item>Bind <c>Authentication:Jwt:{authenticationScheme}</c> vào <see cref="AuthenticationJwtOption"/>.</item>
    /// <item>Có <c>Authority</c> → dùng metadata OIDC; không có → dùng <c>IssuerSigningKeys</c> symmetric.</item>
    /// <item>Validate options lúc startup.</item>
    /// </list>
    /// <para><b>Khi truyền <paramref name="configureOptions"/>:</b> cấu hình JwtBearer hoàn toàn bằng code — dùng test/prototype.</para>
    /// </remarks>
    public static AuthenticationBuilder AddCoreJwtBearer(
        this AuthenticationBuilder builder,
        IConfiguration configuration,
        string authenticationScheme,
        string displayName,
        Action<JwtBearerOptions>? configureOptions = null)
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<AuthenticationJwtOption>, AuthenticationJwtOptionValidator>());

        if (configureOptions != null)
            return builder.AddJwtBearer(authenticationScheme, displayName ?? authenticationScheme, configureOptions);

        var section = configuration.GetSection($"Authentication:Jwt:{authenticationScheme}");
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
            ValidAudiences = authOption.ValidAudiences.Length > 0 ? authOption.ValidAudiences : null,
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
