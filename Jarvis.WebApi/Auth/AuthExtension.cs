using AspNetCore.Authentication.ApiKey;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Jarvis.Shared.Options;
using Microsoft.AspNetCore.Authentication;

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
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        });

        // builder.AddCoreAuthCognito(() => {
        //     return new CognitoOption {

        //     }
        // })

        return builder;
    }

    // public static AuthenticationBuilder AddCoreAuthorization(this IServiceCollection services, IConfiguration configuration)
    // {
    //     var authSection = configuration.GetSection("Authentication");
    //     AuthenticationOption authOption = new AuthenticationOption();
    //     authSection.Bind(authOption);
    //     services.Configure<AuthenticationOption>(authSection);

    //     return services;
    // }

    public static AuthenticationBuilder AddCoreAuthCognito(this AuthenticationBuilder builder, Func<CognitoOption> cognitoConfigure)
    {
        // var cognitoSection = configuration.GetSection("Authentication:Cognito");
        // CognitoOption cognitoOption = new CognitoOption();
        // cognitoSection.Bind(cognitoOption);
        // services.Configure<CognitoOption>(cognitoSection);

        var cognitoOption = cognitoConfigure.Invoke();

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

    // public static IServiceCollection AddCoreAuthCognito(this AuthenticationBuilder builder, IConfiguration configuration)
    // {
    //     var cognitoSection = configuration.GetSection("Authentication:Cognito");
    //     CognitoOption cognitoOption = new CognitoOption();
    //     cognitoSection.Bind(cognitoOption);
    //     services.Configure<CognitoOption>(cognitoSection);

    //     foreach (var userPool in cognitoOption.UserPools)
    //     {
    //         builder.AddJwtBearer(userPool.Key, options =>
    //         {
    //             options.TokenValidationParameters = new TokenValidationParameters { ValidateAudience = false };
    //             options.Authority = $"{cognitoOption.Endpoint}/{userPool.Value}";
    //             options.RequireHttpsMetadata = false;
    //         });
    //     }

    //     services
    //         .AddAuthorization(options =>
    //         {
    //             options.DefaultPolicy = new AuthorizationPolicyBuilder()
    //                 .RequireAuthenticatedUser()
    //                 .AddAuthenticationSchemes(cognitoOption.UserPools.Keys.ToArray())
    //                 .Build();
    //         });

    //     return services;
    // }

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