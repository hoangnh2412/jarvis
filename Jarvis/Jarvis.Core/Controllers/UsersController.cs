using Infrastructure.Database.Entities;
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
using Jarvis.Core.Models.Identity;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Abstractions.Events;
using Jarvis.Core.Models.Events.Users;
using Jarvis.Core.Events.Users;

namespace Jarvis.Core.Controllers
{
    [Authorize]
    [Route("users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IServiceProvider _serviceProvider;

        public UsersController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [HttpGet]
        [Authorize(nameof(CorePolicy.UserPolicy.User_Read))]
        public async Task<IActionResult> PagingAsync(
            [FromQuery] Paging paging,
            [FromServices] IWorkContext workcontext,
            [FromServices] ICoreUnitOfWork uow)
        {
            var context = await workcontext.GetContextAsync(nameof(CorePolicy.UserPolicy.User_Read));

            var repoUser = uow.GetRepository<IUserRepository>();
            var paged = await repoUser.PagingAsync(context, paging);

            var userKeys = paged.Data.Select(x => x.Key).ToList();

            var repoUserInfo = uow.GetRepository<IUserRepository>();
            var infos = (await repoUserInfo.FindUserInfoByKeysAsync(userKeys)).ToDictionary(x => x.Key, x => x);

            var users = new List<UserModel>();
            foreach (var data in paged.Data)
            {
                UserModel user = data;
                if (infos.ContainsKey(data.Key))
                    user.Infos = infos[data.Key];
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

        [HttpGet("{key}")]
        [Authorize(nameof(CorePolicy.UserPolicy.User_Read))]
        public async Task<IActionResult> GetAsync(
            [FromRoute] Guid key,
            [FromServices] IWorkContext workcontext,
            [FromServices] ICoreUnitOfWork uow,
            [FromServices] IEnumerable<IUserInfoService> userInfoServices)
        {
            var context = await workcontext.GetContextAsync(nameof(CorePolicy.UserPolicy.User_Read));
            var repoUser = uow.GetRepository<IUserRepository>();
            var entity = await repoUser.FindUserByKeyAsync(context, key);
            UserModel user = entity;
            if (user == null)
                return NotFound();

            var info = await repoUser.FindUserInfoByKeyAsync(user.Key);
            if (info != null)
                user.Infos = info;

            var repoPermission = uow.GetRepository<IPermissionRepository>();
            user.IdRoles = (await repoPermission.FindRolesByUserAsync(entity.Id)).Select(x => x.RoleId).ToList();

            var claims = await repoUser.GetUserClaimsAsync(entity.Id);
            user.Claims = claims.Select(x => x.ClaimType).ToList();

            var metadatas = new List<JObject>();
            foreach (var item in userInfoServices)
            {
                var metadata = await item.GetAsync(key);
                if (metadata == null)
                    continue;
                metadatas.Add(JObject.Parse(metadata));
            }

            var obj = JsonExtension.MergeObjectsUseReflection(metadatas);
            user.Metadata = JsonConvert.SerializeObject(obj);

            return Ok(user);
        }

        [HttpGet("{key}/claims")]
        [Authorize(nameof(CorePolicy.UserPolicy.User_Read))]
        public async Task<IActionResult> GetUserClaimsAsync(
            [FromRoute] Guid key,
            [FromServices] IWorkContext workcontext,
            [FromServices] ICoreUnitOfWork uow)
        {
            var tenantCode = await workcontext.GetTenantKeyAsync();
            var repoUser = uow.GetRepository<IUserRepository>();
            var claims = await repoUser.GetUserClaimsAsync(key);
            return Ok(claims.Select(x => new PermissionModel
            {
                Key = x.ClaimType,
                Value = x.ClaimValue
            }));
        }

        [HttpPost]
        [Authorize(nameof(CorePolicy.UserPolicy.User_Create))]
        public async Task<IActionResult> PostAsync(
            [FromBody] UserModel model,
            [FromServices] IWorkContext workcontext,
            [FromServices] ICoreUnitOfWork uow,
            [FromServices] IEventFactory eventFactory,
            [FromServices] IIdentityService identityService)
        {
            var tenantKey = workcontext.GetTenantKey();

            // Nếu ko nhập pasword sẽ tự động tạo ngẫu nhiên
            if (string.IsNullOrEmpty(model.Password))
                model.Password = RandomExtension.Random(10);

            // Tạo tài khoản
            var userId = await identityService.CreateAsync(tenantKey, new CreateUserModel
            {
                UserName = model.UserName,
                Password = model.Password,
                FullName = model.Infos.FullName,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email,
                Metadata = model.Metadata,
                Type = Constants.UserType.User
            });

            // Gán vai trò
            var repoUser = uow.GetRepository<IUserRepository>();
            foreach (var roleId in model.IdRoles)
            {
                await repoUser.AssignRoleToUserAsync(userId, roleId);
            }
            await uow.CommitAsync();

            eventFactory.GetOrAddEvent<IEvent<UserCreatedEventModel>, IUserCreatedEvent>().ForEach(async (e) =>
            {
                await e.PublishAsync(new UserCreatedEventModel
                {
                    TenantCode = tenantKey,
                    IdUser = userId,
                    UserName = model.UserName,
                    Password = model.Password,
                    FullName = model.Infos.FullName,
                    Email = model.Email
                });
            });

            return Ok();
        }

        [HttpPut("{key}")]
        [Authorize(nameof(CorePolicy.UserPolicy.User_Update))]
        public async Task<IActionResult> PutAsync(
            [FromRoute] Guid key,
            [FromBody] UserModel model,
            [FromServices] IWorkContext workcontext,
            [FromServices] ICoreUnitOfWork uow,
            [FromServices] IEventFactory eventFactory,
            [FromServices] IEnumerable<IUserInfoService> userInfoServices)
        {
            var context = await workcontext.GetContextAsync(nameof(CorePolicy.UserPolicy.User_Update));
            var repoUser = uow.GetRepository<IUserRepository>();
            var user = await repoUser.FindUserByKeyAsync(context, key);
            if (user == null)
                return NotFound();

            repoUser.UpdateUserFields(user,
                user.Set(x => x.PhoneNumber, model.PhoneNumber),
                user.Set(x => x.Email, model.Email));

            var info = await repoUser.FindUserInfoByKeyAsync(key);
            if (info == null)
                return NotFound();

            // Update user info
            repoUser.UpdateUserInfoFields(info,
                info.Set(x => x.FullName, model.Infos.FullName),
                info.Set(x => x.AvatarPath, model.Infos.AvatarPath));

            // Update role
            var repoUserRole = uow.GetRepository<IPermissionRepository>();
            var roles = await repoUserRole.FindRolesByUserAsync(user.Id);
            var roleKeys = roles.Select(x => x.RoleId);
            repoUserRole.DeleteUserRoles(roles.Where(x => roleKeys.Except(model.IdRoles).Contains(x.RoleId)).ToList());
            await repoUserRole.InsertUserRolesAsync(model.IdRoles.Except(roleKeys).Select(x => new IdentityUserRole<Guid> { RoleId = x, UserId = user.Id }).ToList());

            // Delete token info
            if (roleKeys.Except(model.IdRoles).Any() || model.IdRoles.Except(roleKeys).Any())
            {
                await DeleteTokenAsync(key);
            }

            await uow.CommitAsync();

            foreach (var item in userInfoServices)
            {
                await item.UpdateAsync(key, model.Metadata);
            }

            eventFactory.GetOrAddEvent<IEvent<UserUpdatedEventModel>, IUserUpdatedEvent>().ForEach(async (e) =>
            {
                await e.PublishAsync(new UserUpdatedEventModel
                {
                    TenantCode = context.TenantKey,
                    IdUser = context.UserKey,
                    UserName = model.UserName,
                    FullName = model.Infos.FullName,
                    Email = model.Email
                });
            });

            return Ok();
        }


        [HttpDelete("{key}")]
        [Authorize(nameof(CorePolicy.UserPolicy.User_Delete))]
        public async Task<IActionResult> DeleteAsync(
            [FromRoute] Guid key,
            [FromServices] IWorkContext workcontext,
            [FromServices] ICoreUnitOfWork uow,
            [FromServices] IEventFactory eventFactory,
            [FromServices] IEnumerable<IUserInfoService> userInfoServices)
        {
            var context = await workcontext.GetContextAsync(nameof(CorePolicy.UserPolicy.User_Delete));
            var repoUser = uow.GetRepository<IUserRepository>();
            var user = await repoUser.FindUserByKeyAsync(context, key);
            if (user == null)
                return NotFound("Không tìm thấy tài khoản");

            repoUser.Delete(user);

            // Remove info
            var info = await repoUser.FindUserInfoByKeyAsync(key);
            if (info != null)
                repoUser.DeleteUserInfo(info);

            // Remove roles, claims
            var repoPermission = uow.GetRepository<IPermissionRepository>();
            var roles = await repoPermission.FindRolesByUserAsync(key);
            repoPermission.DeleteUserRoles(roles);

            var claims = await repoPermission.FindUserClaimByUserAsync(key);
            repoPermission.DeleteUserClaims(claims);

            await uow.CommitAsync();

            // Remove token info
            await DeleteTokenAsync(key);
            await uow.CommitAsync();

            foreach (var item in userInfoServices)
            {
                await item.DeleteAsync(key);
            }

            eventFactory.GetOrAddEvent<IEvent<UserDeletedEventModel>, IUserDeletedEvent>().ForEach(async (e) =>
            {
                await e.PublishAsync(new UserDeletedEventModel
                {
                    TenantCode = context.TenantKey,
                    IdUser = context.UserKey,
                    UserName = user.UserName,
                    FullName = info.FullName,
                    Email = user.Email
                });
            });

            return Ok();
        }

        [HttpPatch("{key}/lock/{time?}")]
        [Authorize(nameof(CorePolicy.UserPolicy.User_Lock))]
        public async Task<IActionResult> LockAsync(
            [FromRoute] Guid key,
            [FromRoute] string time,
            [FromServices] IDomainWorkContext workContext,
            [FromServices] IEventFactory eventFactory,
            [FromServices] IIdentityService identityService)
        {
            var tenantKey = workContext.GetTenantKey();
            await identityService.LockAsync(tenantKey, key, time);

            eventFactory.GetOrAddEvent<IEvent<UserLockedEventModel>, IUserLockedEvent>().ForEach(async (e) =>
            {
                await e.PublishAsync(new UserLockedEventModel
                {
                    TenantCode = workContext.GetTenantKey(),
                    IdUser = workContext.GetUserKey(),
                });
            });
            return Ok();
        }

        [HttpPatch("{key}/unlock/{time?}")]
        [Authorize(nameof(CorePolicy.UserPolicy.User_Lock))]
        public async Task<IActionResult> UnlockAsync(
            [FromRoute] Guid key,
            [FromRoute] string time,
            [FromServices] IDomainWorkContext workContext,
            [FromServices] IEventFactory eventFactory,
            [FromServices] IIdentityService identityService)
        {
            var tenantKey = workContext.GetTenantKey();
            await identityService.UnlockAsync(tenantKey, key, time);

            eventFactory.GetOrAddEvent<IEvent<UserUnlockedEventModel>, IUserUnlockedEvent>().ForEach(async (e) =>
            {
                await e.PublishAsync(new UserUnlockedEventModel
                {
                    TenantCode = workContext.GetTenantKey(),
                    IdUser = workContext.GetUserKey(),
                });
            });
            return Ok();
        }


        /// <summary>
        /// Đặt lại mật khẩu tự động và gửi vào email
        /// </summary>
        /// <param name="model"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("reset-password/{id}")]
        [Authorize(nameof(CorePolicy.UserPolicy.User_Reset_Password))]
        public async Task<IActionResult> ResetPassword(
            [FromRoute] Guid id,
            [FromBody] ResetPasswordModel model,
            [FromServices] IDomainWorkContext workContext,
            [FromServices] IEventFactory eventFactory,
            [FromServices] IIdentityService identityService)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tenantKey = workContext.GetTenantKey();

            await identityService.ResetPasswordAsync(tenantKey, id, model.Password, model.Emails);

            eventFactory.GetOrAddEvent<IEvent<UserPasswordResetedEventModel>, IUserPasswordResetedEvent>().ForEach(async (e) =>
            {
                await e.PublishAsync(new UserPasswordResetedEventModel
                {
                    TenantCode = workContext.GetTenantKey(),
                    IdUser = workContext.GetUserKey(),
                    Emails = model.Emails,
                    Password = model.Password
                });
            });
            return Ok();
        }



        private IQueryable<User> Filter(IQueryable<User> query, Paging paging, Guid idTenant)
        {
            //CreatedBy == Guid.Empty là tài khoản Admin tổng, ko cần hiển thị ra danh sách
            query = query.Where(x => x.TenantCode == idTenant && x.CreatedBy != Guid.Empty);

            if (paging.Search != null)
            {
                foreach (var item in paging.Search)
                {
                    query = query.Contains(item.Key, item.Value);
                }
            }

            if (paging.Sort != null)
            {
                query = query.OrderBy(paging.Sort);
            }

            if (paging.Columns != null)
            {
                query = query.Select(paging.Columns
                    .Where(x => x.Value)
                    .Select(x => x.Key)
                    .ToArray());
            }

            return query;
        }

        /// <summary>
        /// xóa token của tài khoản
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task DeleteTokenAsync(Guid idUser)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var uow = scope.ServiceProvider.GetService<ICoreUnitOfWork>();
                //lấy các token của tài khoản và xóa
                var repoTokenInfo = uow.GetRepository<ITokenInfoRepository>();
                var tokenInfos = await repoTokenInfo.QueryByUserAsync(new List<Guid> { idUser });

                var cache = scope.ServiceProvider.GetService<IDistributedCache>();
                foreach (var item in tokenInfos)
                {
                    repoTokenInfo.Delete(item);

                    //xóa token trong cache
                    await cache.RemoveAsync($":TokenInfos:{item.Id}");
                }
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
            [FromServices] IWorkContext workContext,
            [FromServices] ICoreUnitOfWork uow)
        {
            var tenantKey = workContext.GetTenantKey();

            var repoRole = uow.GetRepository<IRoleRepository>();
            var paged = await repoRole.PagingAsync(tenantKey, paging);

            var result = new Paged<RoleModel>
            {
                Data = paged.Data.Select(x => new RoleModel
                {
                    Id = x.Id,
                    Name = x.Name
                }),
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
