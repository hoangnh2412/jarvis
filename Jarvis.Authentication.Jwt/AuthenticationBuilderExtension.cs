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

public static class AuthenticationBuilderExtension
{
    public static AuthenticationBuilder AddCoreJwtBearer(
        this AuthenticationBuilder builder,
        IConfiguration configuration,
        Action<JwtBearerOptions>? configureOptions = null) =>
        builder.AddCoreJwtBearer(configuration, JwtBearerDefaults.AuthenticationScheme, configureOptions);

    public static AuthenticationBuilder AddCoreJwtBearer(
        this AuthenticationBuilder builder,
        IConfiguration configuration,
        string authenticationScheme,
        Action<JwtBearerOptions>? configureOptions = null) =>
        builder.AddCoreJwtBearer(configuration, authenticationScheme, authenticationScheme, configureOptions);

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
