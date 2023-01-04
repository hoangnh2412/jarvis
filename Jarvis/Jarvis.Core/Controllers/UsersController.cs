using Infrastructure.Database.EntityFramework;
using Infrastructure.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Jarvis.Core.Extensions;
using Jarvis.Core.Permissions;
using Jarvis.Core.Database.Repositories;
using Jarvis.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Extensions;
using Jarvis.Models.Identity.Models.Identity;
using Jarvis.Core.Models;
using Microsoft.Extensions.Caching.Distributed;
using Jarvis.Core.Database;
using Jarvis.Core.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Infrastructure.Caching;
using Infrastructure.Abstractions.Events;
using Jarvis.Core.Events.Users;
using Jarvis.Core.Models.Events.Users;
using Jarvis.Core.Models.Identity;

namespace Jarvis.Core.Controllers
{
    [Authorize]
    [Route("users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        [HttpGet]
        [Authorize(nameof(CorePolicy.UserPolicy.User_Read))]
        public async Task<IActionResult> GetAsync(
            [FromQuery] Paging paging,
            [FromServices] IWorkContext workcontext,
            [FromServices] ICoreUnitOfWork uow)
        {
            var context = await workcontext.GetContextAsync(nameof(CorePolicy.UserPolicy.User_Read));

            var repoUser = uow.GetRepository<IUserRepository>();
            var paged = await repoUser.PagingAsync(context, paging);

            var userCodes = paged.Data.Select(x => x.Id).ToList();

            var repoUserInfo = uow.GetRepository<IUserRepository>();
            var infos = (await repoUserInfo.FindInfoByIdsAsync(userCodes)).ToDictionary(x => x.Id, x => x);

            var users = new List<UserModel>();
            foreach (var data in paged.Data)
            {
                UserModel user = data;
                if (infos.ContainsKey(data.Id))
                    user.Infos = infos[data.Id];
                users.Add(user);
            }

            var result = new Paged<UserModel>
            {
                Data = users,
                Page = paged.Page,
                Q = paged.Q,
                Size = paged.Size,
                TotalItems = paged.TotalItems,
                TotalPages = paged.TotalPages
            };
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(nameof(CorePolicy.UserPolicy.User_Read))]
        public async Task<IActionResult> GetAsync(
            [FromRoute] Guid id,
            [FromServices] IWorkContext workcontext,
            [FromServices] ICoreUnitOfWork uow,
            [FromServices] IEnumerable<IUserInfoService> userInfoServices)
        {
            var context = await workcontext.GetContextAsync(nameof(CorePolicy.UserPolicy.User_Read));
            var repoUser = uow.GetRepository<IUserRepository>();
            UserModel user = await repoUser.FindUserByIdAsync(context, id);
            if (user == null)
                return NotFound();

            var info = await repoUser.FindUserInfoByIdAsync(user.Id);
            if (info != null)
            {
                user.Infos = info;
            }

            var repoPermission = uow.GetRepository<IPermissionRepository>();
            user.IdRoles = (await repoPermission.FindRolesByUserAsync(user.Id)).Select(x => x.RoleId).ToList();

            var metadatas = new List<JObject>();
            foreach (var item in userInfoServices)
            {
                var metadata = await item.GetAsync(id);
                if (metadata == null)
                    continue;
                metadatas.Add(JObject.Parse(metadata));
            }

            var obj = JsonExtension.MergeObjectsUseReflection(metadatas);
            user.Metadata = JsonConvert.SerializeObject(obj);

            return Ok(user);
        }

        [HttpPost]
        [Authorize(nameof(CorePolicy.UserPolicy.User_Create))]
        public async Task<IActionResult> PostAsync(
            [FromBody] CreateUserModel command,
            [FromServices] IWorkContext workcontext,
            [FromServices] ICoreUnitOfWork uow,
            [FromServices] IIdentityService identityService,
            [FromServices] IEventFactory eventFactory)
        {
            var tenantCode = await workcontext.GetTenantCodeAsync();

            var isRandomPassword = false;

            //Nếu ko nhập pasword => bắt buộc nhập email và sẽ tự động random password
            if (string.IsNullOrEmpty(command.Password))
            {
                if (string.IsNullOrEmpty(command.Email))
                    throw new Exception("Email tài khoản không được để trống khi chọn password tự động");

                isRandomPassword = true;
                command.Password = RandomExtension.Random(10);
            }

            var idUser = await identityService.CreateAsync(tenantCode, command);

            var repoUser = uow.GetRepository<IUserRepository>();
            foreach (var idRole in command.IdRoles)
            {
                await repoUser.AssignRoleToUserAsync(idUser, idRole);
            }
            await uow.CommitAsync();

            //Notification
            eventFactory.GetOrAddEvent<IEvent<UserCreatedEventModel>, IUserCreatedEvent>().ForEach(async (e) =>
            {
                await e.PublishAsync(new UserCreatedEventModel
                {
                    Email = command.Email,
                    FullName = command.Infos.FullName,
                    IdUser = idUser,
                    Password = command.Password,
                    IsRandomPassword = isRandomPassword,
                    TenantCode = tenantCode,
                    UserName = command.UserName
                });
            });

            return Ok();
        }

        [HttpPut("{id}")]
        [Authorize(nameof(CorePolicy.UserPolicy.User_Update))]
        public async Task<IActionResult> PutAsync(
            [FromRoute] Guid id,
            [FromBody] UpdateUserModel command,
            [FromServices] IWorkContext workcontext,
            [FromServices] ICoreUnitOfWork uow,
            [FromServices] IEnumerable<IUserInfoService> userInfoServices,
            [FromServices] IEventFactory eventFactory,
            [FromServices] ICacheService cacheService)
        {
            var tenantCode = await workcontext.GetTenantCodeAsync();
            var repoUser = uow.GetRepository<IUserRepository>();
            var user = await repoUser.FindUserByIdAsync(tenantCode, id);
            if (user == null)
                return NotFound();

            repoUser.UpdateUserFields(user,
                user.Set(x => x.PhoneNumber, command.PhoneNumber),
                user.Set(x => x.Email, command.Email));

            var info = await repoUser.FindUserInfoByIdAsync(id);
            if (info == null)
                return NotFound();

            //Update user info
            repoUser.UpdateUserInfoFields(info,
                info.Set(x => x.FullName, command.Infos.FullName),
                info.Set(x => x.AvatarPath, command.Infos.AvatarPath));

            //Update role
            var repoUserRole = uow.GetRepository<IPermissionRepository>();
            var roles = await repoUserRole.FindRolesByUserAsync(user.Id);
            var idRoles = roles.Select(x => x.RoleId);

            var deleteUserRoles = roles.Where(x => idRoles.Except(command.IdRoles).Contains(x.RoleId)).ToList();

            repoUserRole.DeleteUserRoles(deleteUserRoles);

            await repoUserRole.InsertUserRolesAsync(command.IdRoles
                .Except(idRoles)
                .Select(x => new IdentityUserRole<Guid>
                {
                    RoleId = x,
                    UserId = user.Id
                })
                .ToList());

            //xóa token của tk nếu sửa quyền
            if (idRoles.Except(command.IdRoles).Any() || command.IdRoles.Except(idRoles).Any())
            {
                await DeleteTokenAsync(uow, cacheService, id);
            }

            await uow.CommitAsync();

            foreach (var item in userInfoServices)
            {
                await item.UpdateAsync(id, command.Metadata);
            }

            //Notification
            eventFactory.GetOrAddEvent<IEvent<UserUpdatedEventModel>, IUserUpdatedEvent>().ForEach((e) =>
            {
                e.PublishAsync(new UserUpdatedEventModel
                {
                    Email = command.Email,
                    FullName = command.Infos.FullName,
                    IdUser = id,
                    TenantCode = tenantCode,
                    UserName = user.UserName
                });
            });

            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(nameof(CorePolicy.UserPolicy.User_Delete))]
        public async Task<IActionResult> DeleteAsync(
            [FromRoute] Guid id,
            [FromServices] IWorkContext workcontext,
            [FromServices] ICoreUnitOfWork uow,
            [FromServices] IEnumerable<IUserInfoService> userInfoServices,
            [FromServices] IEventFactory eventFactory,
            [FromServices] ICacheService cacheService)
        {
            var tenantCode = await workcontext.GetTenantCodeAsync();
            var repoUser = uow.GetRepository<IUserRepository>();
            var user = await repoUser.FindUserByIdAsync(tenantCode, id);
            if (user == null)
                return NotFound("Không tìm thấy tài khoản");

            repoUser.Delete(user);

            var info = await repoUser.FindUserInfoByIdAsync(id);
            if (info != null)
            {
                repoUser.DeleteUserInfo(info);
                await uow.CommitAsync();
            }

            //xóa token của tk 
            await DeleteTokenAsync(uow, cacheService, id);
            await uow.CommitAsync();

            foreach (var item in userInfoServices)
            {
                await item.DeleteAsync(id);
            }

            //Notification
            eventFactory.GetOrAddEvent<IEvent<UserDeletedEventModel>, IUserDeletedEvent>().ForEach(async (e) =>
            {
                await e.PublishAsync(new UserDeletedEventModel
                {
                    Email = user.Email,
                    FullName = info.FullName,
                    IdUser = id,
                    TenantCode = tenantCode,
                    UserName = user.UserName
                });
            });

            return Ok();
        }

        [HttpPatch("{id}/lock/{time?}")]
        [Authorize(nameof(CorePolicy.UserPolicy.User_Lock))]
        public async Task<IActionResult> LockAsync(
            [FromRoute] Guid id,
            [FromRoute] string time,
            [FromServices] IWorkContext workcontext,
            [FromServices] IIdentityService identityService,
            [FromServices] IEventFactory eventFactory)
        {
            var tenantCode = await workcontext.GetTenantCodeAsync();
            await identityService.LockAsync(tenantCode, id, time);

            eventFactory.GetOrAddEvent<IEvent<UserLockedEventModel>, IUserLockedEvent>().ForEach(async (e) =>
            {
                await e.PublishAsync(new UserLockedEventModel
                {
                    IdUser = id,
                    TenantCode = tenantCode
                });
            });

            return Ok();
        }

        [HttpPatch("{id}/unlock/{time?}")]
        [Authorize(nameof(CorePolicy.UserPolicy.User_Lock))]
        public async Task<IActionResult> UnlockAsync(
            [FromRoute] Guid id,
            [FromRoute] string time,
            [FromServices] IWorkContext workcontext,
            [FromServices] IIdentityService identityService,
            [FromServices] IEventFactory eventFactory)
        {
            var tenantCode = await workcontext.GetTenantCodeAsync();
            await identityService.UnlockAsync(tenantCode, id, time);

            eventFactory.GetOrAddEvent<IEvent<UserUnlockedEventModel>, IUserUnlockedEvent>().ForEach(async (e) =>
            {
                await e.PublishAsync(new UserUnlockedEventModel
                {
                    IdUser = id,
                    TenantCode = tenantCode
                });
            });
            return Ok();
        }


        /// <summary>
        /// dat lai mat khau cho tai khoan và gửi mật khẩu vào email
        /// </summary>
        /// <param name="model"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("reset-password/{id}")]
        [Authorize(nameof(CorePolicy.UserPolicy.User_Reset_Password))]
        public async Task<IActionResult> ResetPassword(
            [FromRoute] Guid id,
            [FromBody] ResetPasswordModel model,
            [FromServices] IWorkContext workcontext,
            [FromServices] IIdentityService identityService,
            [FromServices] IEventFactory eventFactory)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tenantCode = await workcontext.GetTenantCodeAsync();

            var password = RandomExtension.Random(10);

            await identityService.ResetPasswordAsync(tenantCode, id, password, model.Emails);

            eventFactory.GetOrAddEvent<IEvent<UserPasswordResetedEventModel>, IUserPasswordResetedEvent>().ForEach(async (e) =>
            {
                await e.PublishAsync(new UserPasswordResetedEventModel
                {
                    IdUser = id,
                    TenantCode = tenantCode,
                    Emails = model.Emails,
                    Password = password
                });
            });
            return Ok();
        }

        /// <summary>
        /// xóa token của tài khoản
        /// </summary>
        /// <param name="idUser"></param>
        /// <returns></returns>
        private async Task DeleteTokenAsync(ICoreUnitOfWork uow, ICacheService cacheService, Guid idUser)
        {
            //lấy các token của tài khoản và xóa
            var repoTokenInfo = uow.GetRepository<ITokenInfoRepository>();
            var tokenInfos = await repoTokenInfo.QueryByUserAsync(new List<Guid> { idUser });

            foreach (var item in tokenInfos)
            {
                repoTokenInfo.Delete(item);

                //xóa token trong cache
                await cacheService.RemoveAsync($":TokenInfos:{item.Id}");
            }
        }

        /// <summary>
        /// lấy danh sách quyền
        /// </summary>
        /// <param name="paging"></param>
        /// <returns></returns>
        [HttpGet("roles")]
        [Authorize(nameof(CorePolicy.UserPolicy.User_Read))]
        public async Task<IActionResult> GetRolesAsync(
            [FromQuery] Paging paging,
            [FromServices] IWorkContext workcontext,
            [FromServices] ICoreUnitOfWork uow)
        {
            var tenantCode = await workcontext.GetTenantCodeAsync();

            var repoRole = uow.GetRepository<IRoleRepository>();
            var paged = await repoRole.PagingAsync(tenantCode, paging);

            var result = new Paged<RoleModel>
            {
                Data = paged.Data.Select(x => (RoleModel)x),
                Page = paged.Page,
                Q = paged.Q,
                Size = paged.Size,
                TotalItems = paged.TotalItems,
                TotalPages = paged.TotalPages
            };
            return Ok(result);
        }
    }
}
