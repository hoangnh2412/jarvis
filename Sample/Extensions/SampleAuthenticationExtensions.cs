using Jarvis.Authentication;
using Jarvis.Authentication.ApiKey;
using Sample.Authentication;

namespace Sample.Extensions;

/// <summary>
/// Demo Sample: ApiKey + <see cref="SampleDemoIdentityOptions"/> (notifications inbox).
/// </summary>
public static class SampleAuthenticationExtensions
{
    public static WebApplicationBuilder AddSampleAuthentication(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;

        builder.Services.Configure<SampleDemoIdentityOptions>(
            configuration.GetSection(SampleDemoIdentityOptions.SectionName));

        builder.Services.AddJarvisAuthentication(configuration, auth =>
        {
            auth.AddCoreApiKey<SampleApiKeyProvider>(configuration);
        });

        return builder;
    }
}
