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
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

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

        // private readonly IEnumerable<IUserInfoService> userInfoServices;
        // private readonly IWorkContext workcontext;
        // private readonly ICoreUnitOfWork uow;
        // private readonly IDistributedCache cache;

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
            var infos = (await repoUserInfo.FindUserInfoByIdsAsync(userCodes)).ToDictionary(x => x.Id, x => x);

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

            var claims = await repoUser.GetUserClaimsAsync(user.Id);
            user.Claims = claims.Select(x => x.ClaimType).ToList();

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

        [HttpGet("{id}/claims")]
        [Authorize(nameof(CorePolicy.UserPolicy.User_Read))]
        public async Task<IActionResult> GetUserClaimsAsync(
            [FromRoute] Guid id,
            [FromServices] IWorkContext workcontext,
            [FromServices] ICoreUnitOfWork uow)
        {
            var tenantCode = await workcontext.GetTenantCodeAsync();
            var repoUser = uow.GetRepository<IUserRepository>();
            var claims = await repoUser.GetUserClaimsAsync(id);
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
            [FromServices] IdentityService identityService)
        {
            var tenantCode = await workcontext.GetTenantCodeAsync();

            //Nếu ko nhập pasword sẽ tự động random
            if (string.IsNullOrEmpty(model.Password))
                model.Password = RandomExtension.Random(10);

            //Tạo tài khoản
            var idUser = await identityService.CreateAsync(tenantCode, new CreateUserModel
            {
                UserName = model.UserName,
                Password = model.Password,
                FullName = model.Infos.FullName,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email,
                Metadata = model.Metadata
            });

            //Gán quyền chức năng
            var repoUser = uow.GetRepository<IUserRepository>();
            foreach (var idRole in model.IdRoles)
            {
                await repoUser.AssignRoleToUserAsync(idUser, idRole);
            }
            await uow.CommitAsync();

            //Gán quyền dữ liệu
            await repoUser.AssignClaimToUserAsync(idUser, model.Claims);
            await uow.CommitAsync();

            //Tạo job send mail

            return Ok();
        }

        [HttpPut("{id}")]
        [Authorize(nameof(CorePolicy.UserPolicy.User_Update))]
        public async Task<IActionResult> PutAsync(
            [FromRoute] Guid id,
            [FromBody] UserModel model,
            [FromServices] IWorkContext workcontext,
            [FromServices] ICoreUnitOfWork uow,
            [FromServices] IEnumerable<IUserInfoService> userInfoServices)
        {
            var context = await workcontext.GetContextAsync(nameof(CorePolicy.UserPolicy.User_Update));
            var repoUser = uow.GetRepository<IUserRepository>();
            var user = await repoUser.FindUserByIdAsync(context, id);
            if (user == null)
                return NotFound();

            repoUser.UpdateUserFields(user,
                user.Set(x => x.PhoneNumber, model.PhoneNumber),
                user.Set(x => x.Email, model.Email));

            var info = await repoUser.FindUserInfoByIdAsync(id);
            if (info == null)
                return NotFound();

            //Update user info
            repoUser.UpdateUserInfoFields(info,
                info.Set(x => x.FullName, model.Infos.FullName),
                info.Set(x => x.AvatarPath, model.Infos.AvatarPath));

            //Update role
            var repoUserRole = uow.GetRepository<IPermissionRepository>();
            var roles = await repoUserRole.FindRolesByUserAsync(user.Id);
            var idRoles = roles.Select(x => x.RoleId);
            repoUserRole.DeleteUserRoles(roles.Where(x => idRoles.Except(model.IdRoles).Contains(x.RoleId)).ToList());
            await repoUserRole.InsertUserRolesAsync(model.IdRoles.Except(idRoles).Select(x => new IdentityUserRole<Guid> { RoleId = x, UserId = user.Id }).ToList());

            //Update claims
            var claims = await repoUser.GetUserClaimsAsync(user.Id);
            var claimTypes = claims.Select(x => x.ClaimType).ToList();
            repoUser.DeleteUserClaim(claims.Where(x => claimTypes.Except(model.Claims).Contains(x.ClaimType)).ToList());
            await repoUser.AssignClaimToUserAsync(user.Id, model.Claims.Except(claimTypes).ToList());

            //xóa token của tk nếu sửa quyền
            if (idRoles.Except(model.IdRoles).Any() || model.IdRoles.Except(idRoles).Any())
            {
                await DeleteTokenAsync(id);
            }

            await uow.CommitAsync();

            foreach (var item in userInfoServices)
            {
                await item.UpdateAsync(id, model.Metadata);
            }

            return Ok();
        }


        [HttpDelete("{id}")]
        [Authorize(nameof(CorePolicy.UserPolicy.User_Delete))]
        public async Task<IActionResult> DeleteAsync(
            [FromRoute] Guid id,
            [FromServices] IWorkContext workcontext,
            [FromServices] ICoreUnitOfWork uow,
            [FromServices] IEnumerable<IUserInfoService> userInfoServices)
        {
            var context = await workcontext.GetContextAsync(nameof(CorePolicy.UserPolicy.User_Delete));
            var repoUser = uow.GetRepository<IUserRepository>();
            var user = await repoUser.FindUserByIdAsync(context, id);
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
            await DeleteTokenAsync(id);
            await uow.CommitAsync();

            foreach (var item in userInfoServices)
            {
                await item.DeleteAsync(id);
            }

            return Ok();
        }

        [HttpPatch("{id}/lock/{time?}")]
        [Authorize(nameof(CorePolicy.UserPolicy.User_Lock))]
        public async Task<IActionResult> LockAsync(
            [FromRoute] Guid id,
            [FromRoute] string time,
            [FromServices] IIdentityService identityService)
        {
            await identityService.LockAsync(id, time);
            return Ok();
        }

        [HttpPatch("{id}/unlock/{time?}")]
        [Authorize(nameof(CorePolicy.UserPolicy.User_Lock))]
        public async Task<IActionResult> UnlockAsync(
            [FromRoute] Guid id,
            [FromRoute] string time,
            [FromServices] IIdentityService identityService)
        {
            await identityService.UnlockAsync(id, time);
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
            [FromServices] IWorkContext workcontext,
            [FromServices] IIdentityService identityService)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tenantCode = await workcontext.GetTenantCodeAsync();

            await identityService.ResetPasswordAsync(tenantCode, id, model.Password, model.Emails);

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
            var tenantCode = await workContext.GetTenantCodeAsync();

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

        /// <summary>
        /// lấy danh sách quyền
        /// </summary>
        /// <param name="paging"></param>
        /// <returns></returns>
        [HttpGet("claims")]
        [Authorize(nameof(CorePolicy.UserPolicy.User_Read))]
        public IActionResult GetClaims()
        {
            var type = typeof(SpecialPolicy);
            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            var fields = fieldInfos.Where(fi => fi.IsLiteral && !fi.IsInitOnly).ToList();
            fields.RemoveAll(x => x.Name == nameof(SpecialPolicy.Special_DoEnything) || x.Name == nameof(SpecialPolicy.Special_TenantAdmin));

            return Ok(fields.Select(x => new PermissionModel
            {
                Key = x.Name,
                Value = x.GetRawConstantValue().ToString(),
            }));
        }
    }
}
