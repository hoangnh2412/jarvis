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
        private readonly IWorkContext _workContext;
        private readonly IIdentityService _identityService;

        public IdentityController(
            IWorkContext workContext,
            IIdentityService identityService)
        {
            _workContext = workContext;
            _identityService = identityService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterModel model)
        {
            var tenantCode = await _workContext.GetTenantCodeAsync();
            await _identityService.RegisterAsync(tenantCode, model);
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginModel model)
        {
            var tenantCode = await _workContext.GetTenantCodeAsync();
            var token = await _identityService.LoginAsync(tenantCode, model);
            return Ok(token);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> LogoutAsync()
        {
            await _identityService.LogoutAsync();
            return Ok();
        }

        [Authorize]
        [HttpGet("session")]
        public async Task<IActionResult> GetSessionAsync()
        {
            var session = await _workContext.GetSessionAsync();
            return Ok(session);
        }

        [HttpGet("refresh-token")]
        public async Task<IActionResult> RefreshTokenAsync([FromQuery] string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return Unauthorized();
            }

            var token = await _identityService.RefreshTokenAsync(refreshToken);
            if (token == null)
                return BadRequest("Token không tồn tại");

            return Ok(token);
        }

        [Authorize]
        [HttpGet("has-claims")]
        public async Task<IActionResult> HasClaimsAsync([FromQuery] List<string> claims)
        {
            if (await _workContext.HasClaimsAsync(claims))
                return Ok();
            return Forbid();
        }

        [Authorize]
        [HttpGet("get-claims")]
        public async Task<IActionResult> GetClaimsAsync([FromQuery] string prefix)
        {
            var claims = await _workContext.GetClaimsAsync(prefix);
            return Ok(claims);
        }

        [Authorize]
        [HttpGet("is-authorize")]
        public async Task<IActionResult> IsAuthorizeAsync()
        {
            var session = await _workContext.GetSessionAsync();
            if (session == null)
                return Unauthorized();
            return Ok();
        }


        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            await _identityService.ForgotPasswordAsync(model);
            return Ok();
        }


        [AllowAnonymous]
        [HttpPost("reset-forgot-password")]
        public async Task<IActionResult> RestForgotPassword([FromBody] ResetForgotPasswordModel model)
        {
            await _identityService.ResetForgotPasswordAsync(model);
            return Ok();
        }
    }
}
