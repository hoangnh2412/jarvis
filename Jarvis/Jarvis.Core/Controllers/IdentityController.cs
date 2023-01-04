using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Jarvis.Core.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jarvis.Models.Identity.Models.Identity;
using Jarvis.Core.Models.Identity;

namespace Jarvis.Core.Controllers
{
    [Route("identity")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(
            [FromBody] RegisterModel model,
            [FromServices] IWorkContext workContext,
            [FromServices] IIdentityService identityService)
        {
            var tenantCode = await workContext.GetTenantCodeAsync();
            await identityService.RegisterAsync(tenantCode, model);
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(
            [FromBody] LoginModel model,
            [FromServices] IWorkContext workContext,
            [FromServices] IIdentityService identityService)
        {
            var tenantCode = await workContext.GetTenantCodeAsync();
            var token = await identityService.LoginAsync(tenantCode, model);
            return Ok(token);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> LogoutAsync(
            [FromServices] IIdentityService identityService
        )
        {
            await identityService.LogoutAsync();
            return Ok();
        }

        [Authorize]
        [HttpGet("session")]
        public async Task<IActionResult> GetSessionAsync(
            [FromServices] IWorkContext workContext
        )
        {
            var session = await workContext.GetSessionAsync();
            return Ok(session);
        }

        [HttpGet("refresh-token")]
        public async Task<IActionResult> RefreshTokenAsync(
            [FromQuery] string refreshToken,
            [FromServices] IIdentityService identityService)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return Unauthorized();
            }

            var token = await identityService.RefreshTokenAsync(refreshToken);
            if (token == null)
                return BadRequest("Token không tồn tại");

            return Ok(token);
        }

        [Authorize]
        [HttpGet("has-claims")]
        public async Task<IActionResult> HasClaimsAsync(
            [FromQuery] List<string> claims,
            [FromServices] IWorkContext workContext)
        {
            if (await workContext.HasClaimsAsync(claims))
                return Ok();
            return Forbid();
        }

        [Authorize]
        [HttpGet("get-claims")]
        public async Task<IActionResult> GetClaimsAsync(
            [FromQuery] string prefix,
            [FromServices] IWorkContext workContext)
        {
            var claims = await workContext.GetClaimsAsync(prefix);
            return Ok(claims);
        }

        [Authorize]
        [HttpGet("is-authorize")]
        public async Task<IActionResult> IsAuthorizeAsync(
            [FromServices] IWorkContext workContext
        )
        {
            var session = await workContext.GetSessionAsync();
            if (session == null)
                return Unauthorized();
            return Ok();
        }


        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(
            [FromBody] ForgotPasswordModel model,
            [FromServices] IIdentityService identityService)
        {
            await identityService.ForgotPasswordAsync(model);
            return Ok();
        }


        [AllowAnonymous]
        [HttpPost("reset-forgot-password")]
        public async Task<IActionResult> RestForgotPassword(
            [FromBody] ResetForgotPasswordModel model,
            [FromServices] IIdentityService identityService)
        {
            await identityService.ResetForgotPasswordAsync(model);
            return Ok();
        }
    }
}
