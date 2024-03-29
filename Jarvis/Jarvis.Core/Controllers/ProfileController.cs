﻿using Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Jarvis.Core.Database;
using Jarvis.Core.Services;
using Jarvis.Core.Database.Repositories;
using System.Threading.Tasks;
using Jarvis.Models.Identity.Models.Identity;
using System;
using Infrastructure.Abstractions.Events;
using Jarvis.Core.Models.Events.Profile;
using Jarvis.Core.Events.Profile;

namespace Jarvis.Core.Controllers
{
    [Authorize]
    [Route("profile")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        [HttpGet("user")]
        public async Task<IActionResult> GetAsync(
            [FromServices] IWorkContext workContext,
            [FromServices] ICoreUnitOfWork uow
        )
        {
            var user = await workContext.GetUserAsync();
            var repoUser = uow.GetRepository<IUserRepository>();
            var info = await repoUser.FindUserInfoByKeyAsync(user.Key);

            var model = new ProfileModel
            {
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FullName = info.FullName,
                AvatarPath = info.AvatarPath
            };
            return Ok(model);
        }

        [HttpPost("user")]
        public async Task<IActionResult> PostAsync(
            [FromBody] ProfileModel model,
            [FromServices] IWorkContext workContext,
            [FromServices] ICoreUnitOfWork uow,
            [FromServices] IEventFactory eventFactory)
        {
            var user = await workContext.GetUserAsync();
            var repoUser = uow.GetRepository<IUserRepository>();

            user.Email = model.Email;
            user.NormalizedEmail = string.IsNullOrEmpty(model.Email) ? null : model.Email.ToUpper();
            user.PhoneNumber = model.PhoneNumber;
            user.UpdatedAt = DateTime.Now;
            user.UpdatedAtUtc = DateTime.UtcNow;
            user.UpdatedBy = user.Key;

            repoUser.Update(user);

            var repoInfo = uow.GetRepository<IUserRepository>();
            var info = await repoInfo.FindUserInfoByKeyAsync(user.Key);
            if (info != null)
            {
                repoInfo.UpdateUserInfoFields(info,
                    info.Set(x => x.FullName, model.FullName),
                    info.Set(x => x.AvatarPath, model.AvatarPath));
            }

            await uow.CommitAsync();

            eventFactory.GetOrAddEvent<IEvent<ProfileUpdatedEventModel>, IProfileUpdatedEvent>().ForEach(async (e) =>
            {
                await e.PublishAsync(new ProfileUpdatedEventModel
                {
                    UserKey = user.Key
                });
            });
            return Ok();
        }

        [HttpDelete("user")]
        public async Task<IActionResult> DeleteAsync(
            [FromServices] IWorkContext workContext,
            [FromServices] IIdentityService identityService,
            [FromServices] IEventFactory eventFactory
        )
        {
            var user = await workContext.GetUserAsync();
            await identityService.DeleteAsync(user.Key);

            eventFactory.GetOrAddEvent<IEvent<ProfileDeletedEventModel>, IProfileDeletedEvent>().ForEach(async (e) =>
            {
                await e.PublishAsync(new ProfileDeletedEventModel
                {
                    UserKey = user.Key
                });
            });
            return Ok();
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePasswordAsync(
            [FromBody] ChangePasswordModel model,
            [FromServices] IWorkContext workContext,
            [FromServices] IIdentityService identityService,
            [FromServices] IEventFactory eventFactory)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tenantKey = workContext.GetTenantKey();
            var userKey = workContext.GetUserKey();
            await identityService.ChangePasswordAsync(tenantKey, userKey, model);

            eventFactory.GetOrAddEvent<IEvent<ProfilePasswordChangedEventModel>, IProfilePasswordChangedEvent>().ForEach(async (e) =>
            {
                await e.PublishAsync(new ProfilePasswordChangedEventModel
                {
                    UserKey = userKey,
                    Password = model.NewPassword
                });
            });
            return Ok();
        }
    }
}
