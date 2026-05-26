using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sample.Controllers;

/// <summary>
/// Endpoint probe kiểm tra trạng thái authentication — dùng cho demo và integration test.
/// </summary>
[ApiController]
[Route("api/_auth-test")]
public class AuthProbeController : ControllerBase
{
    /// <summary>Trả về trạng thái authenticated, scheme và tên user hiện tại (không yêu cầu auth).</summary>
    [HttpGet("whoami")]
    [AllowAnonymous]
    public IActionResult WhoAmI() =>
        Ok(new
        {
            Authenticated = User.Identity?.IsAuthenticated ?? false,
            Scheme = User.Identity?.AuthenticationType,
            Name = User.Identity?.Name
        });
}
