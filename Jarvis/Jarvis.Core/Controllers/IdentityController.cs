using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Jarvis.Core.Services;
using System.Threading.Tasks;
using Jarvis.Models.Identity.Models.Identity;
using Jarvis.Core.Models.Identity;
using Infrastructure.Abstractions.Events;
using Jarvis.Core.Events.Identity;
using Jarvis.Core.Models.Events.Identity;

namespace Jarvis.Core.Controllers
{
    [Route("identity")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(
            [FromBody] RegisterModel model,
            [FromServices] IDomainWorkContext workContext,
            [FromServices] IEventFactory eventFactory,
            [FromServices] IIdentityService identityService)
        {
            var tenantKey = workContext.GetTenantKey();
            var userKey = await identityService.RegisterAsync(tenantKey, model, Constants.UserType.User);

            eventFactory.GetOrAddEvent<IEvent<IdentityRegistedEventModel>, IIdentityRegistedEvent>().ForEach(async (e) =>
            {
                await e.PublishAsync(new IdentityRegistedEventModel
                {
                    UserKey = userKey,
                    UserName = model.UserName,
                    Password = model.Password,
                    FullName = model.FullName,
                });
            });
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(
            [FromBody] LoginModel model,
            [FromServices] IWorkContext workContext,
            [FromServices] IIdentityService identityService)
        {
            var tenant = await workContext.GetCurrentTenantAsync();
            if (tenant == null)
                return StatusCode(500, "Tài khoản hoặc mật khẩu không đúng");

            var token = await identityService.LoginAsync(tenant.Key, model);
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

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPasswordAsync(
            [FromBody] ForgotPasswordModel model,
            [FromServices] IWorkContext workContext,
            [FromServices] IEventFactory eventFactory,
            [FromServices] IIdentityService identityService)
        {
            var tenant = await workContext.GetCurrentTenantAsync();
            if (tenant == null)
                return NotFound("Tài khoản không tồn tại");

            var userKey = await identityService.ForgotPasswordAsync(tenant.Key, model);
            eventFactory.GetOrAddEvent<IEvent<IdentityPasswordForgotedEventModel>, IIdentityPasswordForgotedEvent>().ForEach(async (e) =>
            {
                await e.PublishAsync(new IdentityPasswordForgotedEventModel
                {
                    TenantKey = tenant.Key,
                    UserKey = userKey,
                    UserName = model.UserName,
                    Email = model.Email
                });
            });
            return Ok();
        }


        [AllowAnonymous]
        [HttpPost("reset-forgot-password")]
        public async Task<IActionResult> RestForgotPassword(
            [FromBody] ResetForgotPasswordModel model,
            [FromServices] IWorkContext workContext,
            [FromServices] IEventFactory eventFactory,
            [FromServices] IIdentityService identityService)
        {
            var tenant = await workContext.GetCurrentTenantAsync();
            if (tenant == null)
                return NotFound("Tài khoản không tồn tại");

            var userKey = await identityService.ResetForgotPasswordAsync(tenant.Key, model);
            eventFactory.GetOrAddEvent<IEvent<IdentityPasswordResetedEventModel>, IIdentityPasswordResetedEvent>().ForEach(async (e) =>
            {
                await e.PublishAsync(new IdentityPasswordResetedEventModel
                {
                    TenantKey = tenant.Key,
                    UserKey = userKey,
                    Password = model.NewPassword
                });
            });
            return Ok();
        }
    }
}
