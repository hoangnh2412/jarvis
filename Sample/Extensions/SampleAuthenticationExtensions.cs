using Jarvis.Authentication;
using Jarvis.Authentication.ApiKey;
using Jarvis.Authentication.Basic;
using Jarvis.Authentication.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Sample.Authentication;

namespace Sample.Extensions;

/// <summary>
/// Extension cấu hình authentication cho Sample app — đọc flags từ config và đăng ký scheme tương ứng.
/// </summary>
public static class SampleAuthenticationExtensions
{
    /// <summary>
    /// Bật Jwt/ApiKey/Basic theo <c>Authentication:Schemes</c> hoặc <c>Authentication:Type</c>, tự thêm Composite khi có ≥ 2 scheme.
    /// </summary>
    /// <remarks>
    /// <para><b>Chức năng:</b> wrap <c>AddJarvisAuthentication</c> + satellite <c>AddCore*</c> theo config Sample.</para>
    /// <para><b>Khi nào dùng:</b> gọi một lần trong <c>Program.cs</c> thay vì đăng ký scheme thủ công.
    /// Toggle scheme bằng <c>appsettings.json</c> (<c>Authentication:Schemes:*:Enabled</c> hoặc <c>Authentication:Type</c>).</para>
    /// </remarks>
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
            {
                auth.AddCoreBasic(
                    configuration,
                    SampleBasicAuthDbLookup.Lookup,
                    JarvisAuthenticationSchemes.Basic);
            }
        });

        return builder;
    }
}
