using System.Security.Claims;
using Jarvis.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sample.Controllers;

/// <summary>
/// Endpoint demo để test e2e authentication — endpoint chung qua <c>Composite</c> và endpoint khoá cứng theo từng scheme.
/// </summary>
/// <remarks>
/// <para><b>Cách test</b> (scheme bật theo <c>Authentication:Schemes</c> — mặc định ApiKey + Basic, JWT tắt):</para>
/// <list type="bullet">
/// <item>API Key realm <c>Default</c>: header <c>X-API-KEY: &lt;secret&gt;</c>.</item>
/// <item>API Key realm <c>Integration</c>: header <c>X-API-KEY: Integration:&lt;secret&gt;</c>.</item>
/// <item>Basic: header <c>Authorization: Basic base64(user:pass)</c>.</item>
/// <item>JWT (chỉ khi bật <c>Authentication:Schemes:Jwt</c>): header <c>Authorization: Bearer &lt;token&gt;</c>.</item>
/// </list>
/// <para><b>Lưu ý:</b> endpoint khoá theo scheme (<c>me/api-key</c>, <c>me/basic</c>, <c>me/jwt</c>) yêu cầu scheme đó
/// <b>đã được đăng ký</b>; gọi khi scheme chưa bật sẽ lỗi "No authentication handler for scheme". Dùng <c>me</c> (Composite)
/// nếu muốn endpoint hoạt động với bất kỳ scheme nào đang bật.</para>
/// </remarks>
[ApiController]
[Route("api/auth")]
[Authorize]
public class AuthController : ControllerBase
{
    /// <summary>Không yêu cầu auth — mốc so sánh, luôn 200.</summary>
    [HttpGet("public")]
    [AllowAnonymous]
    public IActionResult Public() =>
        Ok(new
        {
            Authenticated = User.Identity?.IsAuthenticated ?? false,
            Message = "Public endpoint — không cần credential."
        });

    /// <summary>Bất kỳ scheme nào (Composite). Không creds → 401; hợp lệ → thông tin principal.</summary>
    [HttpGet("me")]
    public IActionResult Me() => Ok(PrincipalInfo());

    /// <summary>Chỉ chấp nhận API Key. Header <c>X-API-KEY</c> hợp lệ → 200, ngược lại 401.</summary>
    [HttpGet("me/api-key")]
    [Authorize(AuthenticationSchemes = JarvisAuthenticationSchemes.ApiKey)]
    public IActionResult MeApiKey() => Ok(PrincipalInfo());

    /// <summary>Chỉ chấp nhận HTTP Basic. Header <c>Authorization: Basic</c> hợp lệ → 200, ngược lại 401.</summary>
    [HttpGet("me/basic")]
    [Authorize(AuthenticationSchemes = JarvisAuthenticationSchemes.Basic)]
    public IActionResult MeBasic() => Ok(PrincipalInfo());

    /// <summary>Chỉ chấp nhận JWT Bearer (cần bật scheme Jwt). Token hợp lệ → 200, ngược lại 401.</summary>
    [HttpGet("me/jwt")]
    [Authorize(AuthenticationSchemes = JarvisAuthenticationSchemes.Bearer)]
    public IActionResult MeJwt() => Ok(PrincipalInfo());

    /// <summary>Yêu cầu role <c>User</c> — hợp lệ nhưng thiếu role → 403.</summary>
    [HttpGet("user-area")]
    [Authorize(Roles = "User")]
    public IActionResult UserArea() =>
        Ok(new { Area = "User", Name = User.Identity?.Name });

    /// <summary>Yêu cầu role <c>Admin</c> — dùng để kiểm tra 403 với user chỉ có role <c>User</c>.</summary>
    [HttpGet("admin-area")]
    [Authorize(Roles = "Admin")]
    public IActionResult AdminArea() =>
        Ok(new { Area = "Admin", Name = User.Identity?.Name });

    /// <summary>Trích xuất thông tin principal đã xác thực — dùng chung cho các endpoint <c>me*</c>.</summary>
    private object PrincipalInfo() =>
        new
        {
            Authenticated = true,
            Scheme = User.Identity?.AuthenticationType,
            Name = User.Identity?.Name,
            Roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray(),
            Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToArray()
        };
}
