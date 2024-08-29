using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Jarvis.Authentication.Jwt;

public static class AuthenticationJwtExtension
{
    public static AuthenticationBuilder AddCoreJwtBearer(this AuthenticationBuilder builder, IConfiguration configuration, Action<JwtBearerOptions>? configureOptions = null) => builder.AddCoreJwtBearer(configuration, JwtBearerDefaults.AuthenticationScheme, JwtBearerDefaults.AuthenticationScheme, configureOptions);

    public static AuthenticationBuilder AddCoreJwtBearer(this AuthenticationBuilder builder, IConfiguration configuration, string authenticationScheme, Action<JwtBearerOptions>? configureOptions = null) => builder.AddCoreJwtBearer(configuration, authenticationScheme, authenticationScheme, configureOptions);

    public static AuthenticationBuilder AddCoreJwtBearer(this AuthenticationBuilder builder, IConfiguration configuration, string authenticationScheme, string displayName, Action<JwtBearerOptions>? configureOptions = null)
    {
        if (configureOptions != null)
            return builder.AddJwtBearer(authenticationScheme, displayName ?? authenticationScheme, configureOptions);

        var authOption = configuration.GetSection($"Authentication:Jwt:{authenticationScheme}").Get<AuthenticationJwtOption>();

        // Default validator: https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/wiki/ValidatingTokens
        return builder.AddJwtBearer(authenticationScheme, displayName ?? authenticationScheme, options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                IssuerSigningKeys = authOption?.IssuerSigningKeys.Select(x => new SymmetricSecurityKey(Encoding.ASCII.GetBytes(x))),
                ClockSkew = TimeSpan.Zero,
                ValidateActor = authOption?.ValidateActor ?? false,
                ValidateSignatureLast = authOption?.ValidateSignatureLast ?? true,
                ValidateWithLKG = authOption?.ValidateWithLKG ?? false,
                ValidateTokenReplay = authOption?.ValidateTokenReplay ?? false,
                ValidateAudience = authOption?.ValidateAudience ?? true,
                ValidAudiences = authOption?.ValidAudiences,
                ValidateIssuerSigningKey = authOption?.ValidateIssuerSigningKey ?? true,
                ValidateIssuer = authOption?.ValidateIssuer ?? false,
                ValidIssuers = authOption?.ValidIssuers,
                LifetimeValidator = (DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters) =>
                {
                    if (validationParameters == null)
                        throw LogHelper.LogArgumentNullException(nameof(validationParameters));

                    if (!validationParameters.ValidateLifetime)
                        return true;

                    if (authOption != null && authOption.MaxExpireMinutes > 0)
                    {
                        if (notBefore.HasValue && expires.HasValue && (expires - notBefore).Value.TotalMinutes > authOption.MaxExpireMinutes)
                            throw LogHelper.LogExceptionMessage(new SecurityTokenInvalidLifetimeException($"Token lifetime invalid, lifetime > {authOption.MaxExpireMinutes} minutes") { NotBefore = notBefore, Expires = expires });
                    }

                    if (!expires.HasValue && validationParameters.RequireExpirationTime)
                        throw LogHelper.LogExceptionMessage(new SecurityTokenNoExpirationException("Token not expiration time"));

                    if (notBefore.HasValue && expires.HasValue && (notBefore.Value > expires.Value))
                        throw LogHelper.LogExceptionMessage(new SecurityTokenInvalidLifetimeException("Lifetime invalid, NotBefore > Expire") { NotBefore = notBefore, Expires = expires });

                    DateTime utcNow = DateTime.UtcNow;
                    if (notBefore.HasValue && (notBefore.Value > DateTimeUtil.Add(utcNow, validationParameters.ClockSkew)))
                        throw LogHelper.LogExceptionMessage(new SecurityTokenNotYetValidException("Token lifetime invalid, NotBefore > current time") { NotBefore = notBefore.Value });

                    if (expires.HasValue && (expires.Value < DateTimeUtil.Add(utcNow, validationParameters.ClockSkew.Negate())))
                        throw LogHelper.LogExceptionMessage(new SecurityTokenExpiredException("Token expired, Expire > current time") { Expires = expires.Value });

                    return true;
                }
            };
        });
    }
}