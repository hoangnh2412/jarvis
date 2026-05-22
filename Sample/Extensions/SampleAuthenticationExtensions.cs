using Jarvis.Authentication;
using Jarvis.Authentication.ApiKey;
using Jarvis.Authentication.Basic;
using Jarvis.Authentication.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Sample.Extensions;

public static class SampleAuthenticationExtensions
{
    public static WebApplicationBuilder AddSampleAuthentication(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;

        var jwtEnabled = configuration.GetValue<bool?>("Authentication:Schemes:Jwt:Enabled")
            ?? string.Equals(configuration["Authentication:Type"], "Jwt", StringComparison.OrdinalIgnoreCase);

        var apiKeyEnabled = configuration.GetValue<bool?>("Authentication:Schemes:ApiKey:Enabled")
            ?? string.Equals(configuration["Authentication:Type"], "ApiKey", StringComparison.OrdinalIgnoreCase);

        var basicEnabled = configuration.GetValue<bool?>("Authentication:Schemes:Basic:Enabled") ?? false;

        var useComposite = (jwtEnabled ? 1 : 0) + (apiKeyEnabled ? 1 : 0) + (basicEnabled ? 1 : 0) > 1;

        builder.Services.AddJarvisAuthentication(configuration, auth =>
        {
            if (useComposite)
                auth.AddJarvisCompositeScheme(includeBasic: basicEnabled);

            if (jwtEnabled)
                auth.AddCoreJwtBearer(configuration, JwtBearerDefaults.AuthenticationScheme);

            if (apiKeyEnabled)
                auth.AddCoreApiKey(configuration, JarvisAuthenticationSchemes.ApiKey);

            if (basicEnabled)
                auth.AddCoreBasic(configuration, JarvisAuthenticationSchemes.Basic);
        });

        return builder;
    }
}
