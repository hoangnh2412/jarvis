using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Jarvis.Core.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jarvis.Models.Identity.Models.Identity;
using Jarvis.Core.Models.Identity;
using Infrastructure.Abstractions.Events;
using Jarvis.Core.Models.Events.Identity;
using Jarvis.Core.Events.Identity;
using System;

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
            [FromServices] IIdentityService identityService,
            [FromServices] IEventFactory eventFactory)
        {
            var tenantCode = await workContext.GetTenantCodeAsync();
            await identityService.RegisterAsync(tenantCode, model);

            eventFactory.GetOrAddEvent<IEvent<IdentityRegistedEventModel>, IIdentityRegistedEvent>().ForEach(async (e) =>
            {
                await e.PublishAsync(new IdentityRegistedEventModel
                {
                    UserName = model.Username,
                    Password = model.Password,
                    FullName = model.FullName
                });
            });
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(
            [FromBody] LoginModel model,
            [FromServices] IWorkContext workContext,
            [FromServices] IIdentityService identityService,
            [FromServices] IEventFactory eventFactory)
        {
            var tenantCode = await workContext.GetTenantCodeAsync();
            var token = await identityService.LoginAsync(tenantCode, model);

            eventFactory.GetOrAddEvent<IEvent<IdentityLoginedEventModel>, IIdentityLoginedEvent>().ForEach(async (e) =>
            {
                await e.PublishAsync(new IdentityLoginedEventModel
                {
                    TenantCode = tenantCode,
                    UserName = model.UserName,
                    Password = model.Password
                });
            });
            return Ok(token);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> LogoutAsync(
            [FromServices] IWorkContext workContext,
            [FromServices] IIdentityService identityService,
            [FromServices] IEventFactory eventFactory
        )
        {
            var idUser = workContext.GetUserCode();
            if (idUser == Guid.Empty)
                return Ok();

            await identityService.LogoutAsync(idUser);

            eventFactory.GetOrAddEvent<IEvent<IdentityLogoutedEventModel>, IIdentityLogoutedEvent>().ForEach(async (e) =>
            {
                await e.PublishAsync(new IdentityLogoutedEventModel
                {
                    IdUser = idUser
                });
            });
            return Ok();
        }

        [Authorize]
        [HttpGet("session")]
        public async Task<IActionResult> GetSessionAsync(
            [FromServices] IWorkContext workContext
        )
        {
            var session = await workContext.GetSessionAsync();
            if (session == null)
                return Unauthorized();

            return Ok(session);
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
            [FromServices] IWorkContext workContext,
            [FromServices] IIdentityService identityService,
            [FromServices] IEventFactory eventFactory)
        {
            var tenantCode = await workContext.GetTenantCodeAsync();
            var idUser = await identityService.ForgotPasswordAsync(model);

            eventFactory.GetOrAddEvent<IEvent<IdentityPasswordForgotedEventModel>, IIdentityPasswordForgotedEvent>().ForEach(async (e) =>
            {
                await e.PublishAsync(new IdentityPasswordForgotedEventModel
                {
                    Email = model.Email,
                    TenantCode = tenantCode,
                    UserName = model.UserName,
                    IdUser = idUser,
                    HostName = model.HostName
                });
            });

            return Ok();
        }


        [AllowAnonymous]
        [HttpPost("reset-forgot-password")]
        public async Task<IActionResult> ResetForgotPassword(
            [FromBody] ResetForgotPasswordModel model,
            [FromServices] IIdentityService identityService,
            [FromServices] IEventFactory eventFactory)
        {
            await identityService.ResetForgotPasswordAsync(model);

            eventFactory.GetOrAddEvent<IEvent<IdentityPasswordResetedEventModel>, IIdentityPasswordResetedEvent>().ForEach(async (e) =>
            {
                await e.PublishAsync(new IdentityPasswordResetedEventModel
                {
                    IdUser = model.Id,
                    Password = model.NewPassword
                });
            });
            return Ok();
        }
    }
}
