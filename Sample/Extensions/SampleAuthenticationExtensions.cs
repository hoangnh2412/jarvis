using Microsoft.Extensions.DependencyInjection.Extensions;
using Platform.Authentication;
using Platform.Authentication.ApiKey;
using Platform.Authentication.Basic;
using Platform.Authentication.Jwt;
using Sample.Authentication;

namespace Sample.Extensions;

/// <summary>
/// Đăng ký scheme theo <c>Authentication:Schemes:*</c> — <c>Source</c> theo từng profile trong <c>Profiles</c>.
/// </summary>
public static class SampleAuthenticationExtensions
{
    public static WebApplicationBuilder AddSampleAuthentication(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        var root = configuration.GetSection("Authentication").Get<AuthenticationRootOptions>() ?? new AuthenticationRootOptions();

        builder.Services.AddSingleton<IPasswordPolicyScopeProvider, SamplePasswordPolicyScopeProvider>();
        builder.Services.AddSingleton<IPasswordPolicyProvider, TenantPasswordPolicyDbProvider>();
        builder.Services.AddSingleton<DevBearerTokenIssuer>();
        builder.Services.AddScoped<SampleBearerLoginService>();

        builder.Services.AddPlatformAuthentication(configuration, auth =>
        {
            if (root.Schemes.ApiKey.Enabled)
            {
                var apiKeyProfile = AuthenticationSchemeProfileConfiguration.ResolveApiKeyProfile(root, configuration);
                auth.AddCoreApiKeyWithProfileSources<ApiKeyDbProvider>(configuration, apiKeyProfile);
            }

            if (root.Schemes.Bearer.Enabled)
            {
                var bearerProfile = AuthenticationSchemeProfileConfiguration.ResolveBearerProfile(root, configuration);
                var bearerSection = configuration.GetSection(AuthenticationSchemeProfileConfiguration.ProfileSection("Bearer", bearerProfile));
                var bearerOptions = bearerSection.Get<AuthenticationJwtOption>() ?? new AuthenticationJwtOption();
                if (bearerOptions.Source == AuthenticationCredentialSource.Storage)
                    auth.Services.TryAddScoped<IBasicCredentialProvider, BasicAuthDbCredentialProvider>();

                auth.AddCoreJwtBearer(configuration, PlatformAuthenticationSchemes.Bearer, bearerProfile);
            }

            if (root.Schemes.Basic.Enabled)
            {
                var basicProfile = AuthenticationSchemeProfileConfiguration.ResolveBasicProfile(root, configuration);
                var basicScheme = AuthenticationSchemeProfileConfiguration.ResolveBasicScheme(root, configuration);
                auth.AddCoreBasicWithProfileSources<BasicAuthDbCredentialProvider>(configuration, basicScheme, basicProfile);
            }
        });

        return builder;
    }
}
