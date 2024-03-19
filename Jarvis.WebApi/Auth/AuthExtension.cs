using AspNetCore.Authentication.ApiKey;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Jarvis.Shared.Options;

namespace Jarvis.WebApi.Auth;

public static class AuthExtension
{
    public static IServiceCollection AddCoreAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var authSection = configuration.GetSection("Authentication");
        var cognitoSection = configuration.GetSection("Authentication:Cognito");
        AuthenticationOption authOption = new AuthenticationOption();
        authSection.Bind(authOption);

        services.Configure<AuthenticationOption>(authSection);
        services.Configure<CognitoOption>(cognitoSection);

        var builder = services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        });

        foreach (var userPool in authOption.Cognito.UserPools)
        {
            builder.AddJwtBearer(userPool.Key, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters { ValidateAudience = false };
                options.Authority = $"{authOption.Cognito.Endpoint}/{userPool.Value}";
                options.RequireHttpsMetadata = false;
            });
        }

        services
            .AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(authOption.Cognito.UserPools.Keys.ToArray())
                    .Build();
            });

        return services;
    }

    public static IServiceCollection AddAuthApiKey<T>(this IServiceCollection services) where T : class, IApiKeyProvider
    {
        services
            .AddAuthentication(ApiKeyDefaults.AuthenticationScheme)
            .AddApiKeyInHeader<T>(options =>
            {
                options.Realm = "Jarvis";
                options.KeyName = "X-API-KEY";
            });

        return services;
    }

    public static IApplicationBuilder UseCoreAuth(this IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }
}