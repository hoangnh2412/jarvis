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
        private readonly IWorkContext _workContext;
        private readonly IEventFactory _eventFactory;
        private readonly IIdentityService _identityService;

        public IdentityController(
            IWorkContext workContext,
            IEventFactory eventFactory,
            IIdentityService identityService)
        {
            _workContext = workContext;
            _identityService = identityService;
            _eventFactory = eventFactory;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterModel model)
        {
            var tenantCode = await _workContext.GetTenantCodeAsync();
            await _identityService.RegisterAsync(tenantCode, model);

            _eventFactory.GetOrAddEvent<IEvent<IdentityRegistedEventModel>, IIdentityRegistedEvent>().ForEach(async (e) =>
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
        public async Task<IActionResult> LoginAsync([FromBody] LoginModel model)
        {
            var tenantCode = await _workContext.GetTenantCodeAsync();
            var token = await _identityService.LoginAsync(tenantCode, model);

            _eventFactory.GetOrAddEvent<IEvent<IdentityLoginedEventModel>, IIdentityLoginedEvent>().ForEach(async (e) =>
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
        public async Task<IActionResult> LogoutAsync()
        {
            var idUser = _workContext.GetUserCode();
            if (idUser == Guid.Empty)
                return Ok();

            await _identityService.LogoutAsync(idUser);

            _eventFactory.GetOrAddEvent<IEvent<IdentityLogoutedEventModel>, IIdentityLogoutedEvent>().ForEach(async (e) =>
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
        public async Task<IActionResult> GetSessionAsync()
        {
            var session = await _workContext.GetSessionAsync();
            if (session == null)
                return Unauthorized();

            return Ok(session);
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
            var tenantCode = await _workContext.GetTenantCodeAsync();
            await _identityService.ForgotPasswordAsync(model);

            _eventFactory.GetOrAddEvent<IEvent<IdentityPasswordForgotedEventModel>, IIdentityPasswordForgotedEvent>().ForEach(async (e) =>
            {
                await e.PublishAsync(new IdentityPasswordForgotedEventModel
                {
                    Email = model.Email,
                    TenantCode = tenantCode,
                    UserName = model.UserName
                });
            });
            return Ok();
        }


        [AllowAnonymous]
        [HttpPost("reset-forgot-password")]
        public async Task<IActionResult> ResetForgotPassword([FromBody] ResetForgotPasswordModel model)
        {
            await _identityService.ResetForgotPasswordAsync(model);

            _eventFactory.GetOrAddEvent<IEvent<IdentityPasswordResetedEventModel>, IIdentityPasswordResetedEvent>().ForEach(async (e) =>
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
