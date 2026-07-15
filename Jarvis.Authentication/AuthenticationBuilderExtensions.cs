using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.Authentication;

/// <summary>
/// Extension đăng ký policy scheme <c>Composite</c> — forward request sang ApiKey, Basic hoặc Bearer theo header.
/// </summary>
public static class AuthenticationBuilderExtensions
{
    /// <summary>
    /// Thêm scheme <see cref="JarvisAuthenticationSchemes.Composite"/> với forward selector theo header request.
    /// </summary>
    /// <remarks>
    /// <para><b>Chức năng:</b> một endpoint chấp nhận nhiều loại credential — ưu tiên ApiKey header,
    /// sau đó Basic (nếu bật), cuối cùng Bearer JWT.</para>
    /// <para><b>Khi nào dùng:</b> host bật đồng thời ≥ 2 scheme (Jwt + ApiKey, hoặc thêm Basic).
    /// Đặt <c>DefaultAuthenticateScheme</c> = <c>Composite</c> trong config root.</para>
    /// <para><paramref name="bearerScheme"/> phải trùng tên scheme JWT đã đăng ký qua <c>AddCoreJwtBearer</c>
    /// — truyền vào khi host dùng scheme JWT không phải <c>Bearer</c> mặc định.</para>
    /// </remarks>
    public static AuthenticationBuilder AddJarvisCompositeScheme(
        this AuthenticationBuilder builder,
        string apiKeyHeaderName = "X-API-KEY",
        bool includeBasic = false,
        string bearerScheme = JarvisAuthenticationSchemes.Bearer)
    {
        return builder.AddPolicyScheme(
            JarvisAuthenticationSchemes.Composite,
            "Jarvis authentication",
            options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    if (context.Request.Headers.ContainsKey(apiKeyHeaderName))
                        return JarvisAuthenticationSchemes.ApiKey;

                    var authorization = context.Request.Headers.Authorization.ToString();
                    if (includeBasic && authorization.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                        return JarvisAuthenticationSchemes.Basic;

                    if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        return bearerScheme;

                    return includeBasic
                        ? JarvisAuthenticationSchemes.Basic
                        : bearerScheme;
                };
            });
    }
}
