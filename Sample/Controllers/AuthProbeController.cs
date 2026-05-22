using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sample.Controllers;

[ApiController]
[Route("api/_auth-test")]
public class AuthProbeController : ControllerBase
{
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
