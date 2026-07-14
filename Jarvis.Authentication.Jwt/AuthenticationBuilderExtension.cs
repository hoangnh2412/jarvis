using System.Net.Http.Headers;
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
    /// Đăng ký JwtBearer với scheme mặc định <c>Bearer</c> và <see cref="AllowAllJwtTokenAccessChecker"/>.
    /// </summary>
    public static AuthenticationBuilder AddCoreJwtBearer(
        this AuthenticationBuilder builder,
        IConfiguration configuration,
        Action<JwtBearerOptions>? configureOptions = null) =>
        builder.AddCoreJwtBearer<AllowAllJwtTokenAccessChecker>(
            configuration, JwtBearerDefaults.AuthenticationScheme, configureOptions);

    /// <summary>
    /// Đăng ký JwtBearer với tên scheme tùy chỉnh và <see cref="AllowAllJwtTokenAccessChecker"/>.
    /// </summary>
    public static AuthenticationBuilder AddCoreJwtBearer(
        this AuthenticationBuilder builder,
        IConfiguration configuration,
        string authenticationScheme,
        Action<JwtBearerOptions>? configureOptions = null) =>
        builder.AddCoreJwtBearer<AllowAllJwtTokenAccessChecker>(
            configuration, authenticationScheme, authenticationScheme, configureOptions);

    /// <summary>
    /// Overload đầy đủ với <see cref="AllowAllJwtTokenAccessChecker"/>.
    /// </summary>
    public static AuthenticationBuilder AddCoreJwtBearer(
        this AuthenticationBuilder builder,
        IConfiguration configuration,
        string authenticationScheme,
        string displayName,
        Action<JwtBearerOptions>? configureOptions = null) =>
        builder.AddCoreJwtBearer<AllowAllJwtTokenAccessChecker>(
            configuration, authenticationScheme, displayName, configureOptions);

    /// <summary>
    /// Đăng ký JwtBearer + <typeparamref name="TAccessChecker"/> (blacklist/whitelist sau validate).
    /// </summary>
    /// <remarks>
    /// <para><typeparamref name="TAccessChecker"/> đăng ký Singleton (<see cref="IJwtTokenAccessChecker"/>).</para>
    /// <para>Hook <c>OnTokenValidated</c>: chữ ký OK → <c>IsAllowedAsync</c>; <c>false</c> → Fail.</para>
    /// <example>
    /// <code>
    /// auth.AddCoreJwtBearer&lt;AllowAllJwtTokenAccessChecker&gt;(config);
    /// auth.AddCoreJwtBearer&lt;RedisJwtRevocationChecker&gt;(config);
    /// </code>
    /// </example>
    /// </remarks>
    public static AuthenticationBuilder AddCoreJwtBearer<TAccessChecker>(
        this AuthenticationBuilder builder,
        IConfiguration configuration,
        Action<JwtBearerOptions>? configureOptions = null)
        where TAccessChecker : class, IJwtTokenAccessChecker =>
        builder.AddCoreJwtBearer<TAccessChecker>(
            configuration, JwtBearerDefaults.AuthenticationScheme, configureOptions);

    /// <summary>
    /// Đăng ký JwtBearer với scheme tùy chỉnh + <typeparamref name="TAccessChecker"/>.
    /// </summary>
    public static AuthenticationBuilder AddCoreJwtBearer<TAccessChecker>(
        this AuthenticationBuilder builder,
        IConfiguration configuration,
        string authenticationScheme,
        Action<JwtBearerOptions>? configureOptions = null)
        where TAccessChecker : class, IJwtTokenAccessChecker =>
        builder.AddCoreJwtBearer<TAccessChecker>(
            configuration, authenticationScheme, authenticationScheme, configureOptions);

    /// <summary>
    /// Overload đầy đủ: bind config, đăng ký checker, gắn <c>OnTokenValidated</c>.
    /// </summary>
    /// <remarks>
    /// <para><b>Khi <paramref name="configureOptions"/> là <c>null</c> (khuyến nghị):</b></para>
    /// <list type="bullet">
    /// <item>Bind <c>Authentication:Jwt:{authenticationScheme}</c> vào <see cref="AuthenticationJwtOption"/>.</item>
    /// <item>Có <c>Authority</c> → dùng metadata OIDC; không có → dùng <c>IssuerSigningKeys</c> symmetric.</item>
    /// <item>Validate options lúc startup.</item>
    /// </list>
    /// <para><b>Khi truyền <paramref name="configureOptions"/>:</b> cấu hình JwtBearer bằng code — vẫn gắn access checker.</para>
    /// </remarks>
    public static AuthenticationBuilder AddCoreJwtBearer<TAccessChecker>(
        this AuthenticationBuilder builder,
        IConfiguration configuration,
        string authenticationScheme,
        string displayName,
        Action<JwtBearerOptions>? configureOptions = null)
        where TAccessChecker : class, IJwtTokenAccessChecker
    {
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<AuthenticationJwtOption>, AuthenticationJwtOptionValidator>());
        builder.Services.TryAddSingleton<IJwtTokenAccessChecker, TAccessChecker>();

        if (configureOptions != null)
        {
            return builder.AddJwtBearer(authenticationScheme, displayName ?? authenticationScheme, options =>
            {
                configureOptions(options);
                AttachTokenAccessChecker(options);
            });
        }

        var section = configuration.GetSection($"Authentication:Jwt:{authenticationScheme}");
        builder.Services.Configure<AuthenticationJwtOption>(authenticationScheme, section);
        builder.Services.AddOptions<AuthenticationJwtOption>(authenticationScheme)
            .Bind(section)
            .ValidateOnStart();

        var authOption = section.Get<AuthenticationJwtOption>() ?? new AuthenticationJwtOption();

        return builder.AddJwtBearer(authenticationScheme, displayName ?? authenticationScheme, options =>
        {
            ConfigureJwtBearer(options, authOption, builder.Services);
            AttachTokenAccessChecker(options);
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

    /// <summary>Gắn <see cref="IJwtTokenAccessChecker"/> vào <c>OnTokenValidated</c> (wrap handler có sẵn).</summary>
    public static void AttachTokenAccessChecker(JwtBearerOptions options)
    {
        options.Events ??= new JwtBearerEvents();
        var prior = options.Events.OnTokenValidated;
        options.Events.OnTokenValidated = async context =>
        {
            if (prior is not null)
                await prior(context).ConfigureAwait(false);

            if (context.Result != null)
                return;

            if (context.Principal is null)
                return;

            var checker = context.HttpContext.RequestServices.GetRequiredService<IJwtTokenAccessChecker>();
            var rawToken = TryGetBearerToken(context.Request);
            var allowed = await checker
                .IsAllowedAsync(context.Principal, rawToken, context.HttpContext.RequestAborted)
                .ConfigureAwait(false);

            if (!allowed)
                context.Fail("Token is revoked or not allowed.");
        };
    }

    private static string? TryGetBearerToken(Microsoft.AspNetCore.Http.HttpRequest request)
    {
        if (!AuthenticationHeaderValue.TryParse(request.Headers.Authorization, out var header))
            return null;

        if (!string.Equals(header.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase))
            return null;

        return string.IsNullOrWhiteSpace(header.Parameter) ? null : header.Parameter;
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
