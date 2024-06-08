using AspNetCore.Authentication.ApiKey;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Jarvis.Shared.Options;
using Microsoft.AspNetCore.Authentication;
using System.Text;

namespace Jarvis.WebApi.Auth;

public static class AuthExtension
{
    public static AuthenticationBuilder AddCoreAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var authSection = configuration.GetSection("Authentication");
        AuthenticationOption authOption = new AuthenticationOption();
        authSection.Bind(authOption);
        services.Configure<AuthenticationOption>(authSection);

        var builder = services.AddAuthentication(options =>
        {
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        });

        return builder;
    }

    public static AuthenticationBuilder AddCoreAuthJWT(this AuthenticationBuilder builder, IConfiguration configuration)
    {
        var cognitoSection = configuration.GetSection("Authentication:JWT");
        JWTOption jwtOption = new JWTOption();
        cognitoSection.Bind(jwtOption);
        builder.Services.Configure<JWTOption>(cognitoSection);

        foreach (var schema in jwtOption.Schemas)
        {
            builder.AddJwtBearer(schema.Key, options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = schema.Value.ValidateAudience,
                    ValidateIssuer = schema.Value.ValidateIssuer,
                    ValidateActor = schema.Value.ValidateActor,
                    ValidateLifetime = schema.Value.ValidateLifetime,
                    ValidateIssuerSigningKey = schema.Value.ValidateIssuerSigningKey,
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(schema.Value.SecretKey))
                };
            });
        }

        return builder;
    }

    public static AuthenticationBuilder AddCoreAuthCognito(this AuthenticationBuilder builder, IConfiguration configuration)
    {
        var cognitoSection = configuration.GetSection("Authentication:Cognito");
        CognitoOption cognitoOption = new CognitoOption();
        cognitoSection.Bind(cognitoOption);
        builder.Services.Configure<CognitoOption>(cognitoSection);

        foreach (var userPool in cognitoOption.UserPools)
        {
            builder.AddJwtBearer(userPool.Key, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters { ValidateAudience = false };
                options.Authority = $"{cognitoOption.Endpoint}/{userPool.Value}";
                options.RequireHttpsMetadata = false;
            });
        }

        return builder;
    }

    public static AuthenticationBuilder AddApiKeyInHeader<T>(
        this AuthenticationBuilder builder,
        string authenticationScheme = ApiKeyDefaults.AuthenticationScheme,
        string realm = "Jarvis",
        string keyName = "X-API-KEY")
        where T : class, IApiKeyProvider
    {
        builder.AddApiKeyInHeader<T>(
            authenticationScheme,
            options =>
            {
                options.Realm = realm;
                options.KeyName = keyName;
            });

        return builder;
    }

    public static AuthenticationBuilder AddApiKeyInQueryParams<T>(
        this AuthenticationBuilder builder,
        string authenticationScheme = ApiKeyDefaults.AuthenticationScheme,
        string realm = "Jarvis",
        string keyName = "ApiKey")
        where T : class, IApiKeyProvider
    {
        builder.AddApiKeyInQueryParams<T>(
            authenticationScheme,
            options =>
            {
                options.Realm = realm;
                options.KeyName = keyName;
            });

        return builder;
    }

    public static AuthenticationBuilder AddApiKeyInRouteValues<T>(
        this AuthenticationBuilder builder,
        string authenticationScheme = ApiKeyDefaults.AuthenticationScheme,
        string realm = "Jarvis",
        string keyName = "apiKey")
        where T : class, IApiKeyProvider
    {
        builder.AddApiKeyInRouteValues<T>(
            authenticationScheme,
            options =>
            {
                options.Realm = realm;
                options.KeyName = keyName;
            });

        return builder;
    }

    public static AuthenticationBuilder AddApiKeyInAuthorizationHeader<T>(
        this AuthenticationBuilder builder,
        string authenticationScheme = ApiKeyDefaults.AuthenticationScheme,
        string realm = "Jarvis",
        string keyName = "X-API-KEY")
        where T : class, IApiKeyProvider
    {
        builder.AddApiKeyInAuthorizationHeader<T>(
            authenticationScheme,
            options =>
            {
                options.Realm = realm;
                options.KeyName = keyName;
            });

        return builder;
    }
}