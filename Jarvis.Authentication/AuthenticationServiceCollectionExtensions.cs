using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Jarvis.Authentication;

public static class AuthenticationServiceCollectionExtensions
{
    public static AuthenticationBuilder AddJarvisAuthentication(
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
