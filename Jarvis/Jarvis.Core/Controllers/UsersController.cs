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

namespace Jarvis.Core.Controllers
{
    [Authorize]
    [Route("users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IIdentityService _identityService;
        private readonly IEnumerable<IUserInfoService> _userInfoServices;
        private readonly IWorkContext _workcontext;
        private readonly ICoreUnitOfWork _uow;
        private readonly IDistributedCache _cache;

        public UsersController(
            IIdentityService identityService,
            IEnumerable<IUserInfoService> userInfoServices,
            IWorkContext workcontext,
            ICoreUnitOfWork uow,
            IDistributedCache cache)
        {
            _identityService = identityService;
            _userInfoServices = userInfoServices;
            _workcontext = workcontext;
            _uow = uow;
            _cache = cache;
        }

        [HttpGet]
        [Authorize(nameof(CorePolicy.UserPolicy.User_Read))]
        public async Task<IActionResult> GetAsync([FromQuery] Paging paging)
        {
            var context = await _workcontext.GetContextAsync(nameof(CorePolicy.UserPolicy.User_Read));

            var repoUser = _uow.GetRepository<IUserRepository>();
            var paged = await repoUser.PagingAsync(context, paging);

            var userCodes = paged.Data.Select(x => x.Id).ToList();

            var repoUserInfo = _uow.GetRepository<IUserRepository>();
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
        public async Task<IActionResult> GetAsync([FromRoute] Guid id)
        {
            var context = await _workcontext.GetContextAsync(nameof(CorePolicy.UserPolicy.User_Read));
            var repoUser = _uow.GetRepository<IUserRepository>();
            UserModel user = await repoUser.FindUserByIdAsync(context, id);
            if (user == null)
                return NotFound();

            var info = await repoUser.FindUserInfoByIdAsync(user.Id);
            if (info != null)
            {
                user.Infos = info;
            }

            var repoPermission = _uow.GetRepository<IPermissionRepository>();
            user.IdRoles = (await repoPermission.FindRolesByUserAsync(user.Id)).Select(x => x.RoleId).ToList();

            var claims = await repoUser.GetUserClaimsAsync(user.Id);
            user.Claims = claims.Select(x => x.ClaimType).ToList();

            var metadatas = new List<JObject>();
            foreach (var item in _userInfoServices)
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
        public async Task<IActionResult> GetUserClaimsAsync([FromRoute] Guid id)
        {
            var tenantCode = await _workcontext.GetTenantCodeAsync();
            var repoUser = _uow.GetRepository<IUserRepository>();
            var claims = await repoUser.GetUserClaimsAsync(id);
            return Ok(claims.Select(x => new PermissionModel
            {
                Key = x.ClaimType,
                Value = x.ClaimValue
            }));
        }

        [HttpPost]
        [Authorize(nameof(CorePolicy.UserPolicy.User_Create))]
        public async Task<IActionResult> PostAsync([FromBody] UserModel model)
        {
            var tenantCode = await _workcontext.GetTenantCodeAsync();

            //Nếu ko nhập pasword sẽ tự động random
            if (string.IsNullOrEmpty(model.Password))
                model.Password = RandomExtension.Random(10);

            //Tạo tài khoản
            var idUser = await _identityService.CreateAsync(tenantCode, new CreateUserModel
            {
                Username = model.UserName,
                Password = model.Password,
                FullName = model.Infos.FullName,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email,
                Metadata = model.Metadata
            });

            //Gán quyền chức năng
            var repoUser = _uow.GetRepository<IUserRepository>();
            foreach (var idRole in model.IdRoles)
            {
                await repoUser.AssignRoleToUserAsync(idUser, idRole);
            }
            await _uow.CommitAsync();

            //Gán quyền dữ liệu
            await repoUser.AssignClaimToUserAsync(idUser, model.Claims);
            await _uow.CommitAsync();

            //Tạo job send mail

            return Ok();
        }

        [HttpPut("{id}")]
        [Authorize(nameof(CorePolicy.UserPolicy.User_Update))]
        public async Task<IActionResult> PutAsync([FromRoute] Guid id, [FromBody] UserModel model)
        {
            var context = await _workcontext.GetContextAsync(nameof(CorePolicy.UserPolicy.User_Update));
            var repoUser = _uow.GetRepository<IUserRepository>();
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
            var repoUserRole = _uow.GetRepository<IPermissionRepository>();
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

            await _uow.CommitAsync();

            foreach (var item in _userInfoServices)
            {
                await item.UpdateAsync(id, model.Metadata);
            }

            return Ok();
        }


        [HttpDelete("{id}")]
        [Authorize(nameof(CorePolicy.UserPolicy.User_Delete))]
        public async Task<IActionResult> DeleteAsync([FromRoute] Guid id)
        {
            var context = await _workcontext.GetContextAsync(nameof(CorePolicy.UserPolicy.User_Delete));
            var repoUser = _uow.GetRepository<IUserRepository>();
            var user = await repoUser.FindUserByIdAsync(context, id);
            if (user == null)
                return NotFound("Không tìm thấy tài khoản");

            repoUser.Delete(user);

            var info = await repoUser.FindUserInfoByIdAsync(id);
            if (info != null)
            {
                repoUser.DeleteUserInfo(info);
                await _uow.CommitAsync();
            }

            //xóa token của tk 
            await DeleteTokenAsync(id);
            await _uow.CommitAsync();

            foreach (var item in _userInfoServices)
            {
                await item.DeleteAsync(id);
            }

            return Ok();
        }

        [HttpPatch("{id}/lock/{time?}")]
        [Authorize(nameof(CorePolicy.UserPolicy.User_Lock))]
        public async Task<IActionResult> LockAsync([FromRoute] Guid id, [FromRoute] string time)
        {
            await _identityService.LockAsync(id, time);
            return Ok();
        }

        [HttpPatch("{id}/unlock/{time?}")]
        [Authorize(nameof(CorePolicy.UserPolicy.User_Lock))]
        public async Task<IActionResult> UnlockAsync([FromRoute] Guid id, [FromRoute] string time)
        {
            await _identityService.UnlockAsync(id, time);
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
        public async Task<IActionResult> ResetPassword([FromRoute] Guid id, [FromBody] ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tenantCode = await _workcontext.GetTenantCodeAsync();

            await _identityService.ResetPasswordAsync(tenantCode, id, model.Emails);

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
            //lấy các token của tài khoản và xóa
            var repoTokenInfo = _uow.GetRepository<ITokenInfoRepository>();
            var tokenInfos = await repoTokenInfo.QueryByUserAsync(new List<Guid> { idUser });

            foreach (var item in tokenInfos)
            {
                repoTokenInfo.Delete(item);

                //xóa token trong cache
                await _cache.RemoveAsync($":TokenInfos:{item.Id}");
            }
        }


        /// <summary>
        /// lấy danh sách quyền
        /// </summary>
        /// <param name="paging"></param>
        /// <returns></returns>
        [HttpGet("roles")]
        [Authorize(nameof(CorePolicy.UserPolicy.User_Read))]
        public async Task<IActionResult> GetRolesAsync([FromQuery] Paging paging)
        {
            var tenantCode = await _workcontext.GetTenantCodeAsync();

            var repoRole = _uow.GetRepository<IRoleRepository>();
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

            return Ok(fields.Select(x => new PermissionModel
            {
                Key = x.Name,
                Value = x.GetRawConstantValue().ToString(),
            }));
        }
    }
}
