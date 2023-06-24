using Infrastructure.Database.Entities;
using Infrastructure.Database.EntityFramework;
using Infrastructure.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Jarvis.Core.Constants;
using Jarvis.Core.Permissions;
using Jarvis.Core.Services;
using Jarvis.Core.Database.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jarvis.Core.Database;
using Jarvis.Models.Identity.Models.Identity;
using Infrastructure.Abstractions.Events;
using Jarvis.Core.Models.Events.Roles;
using Jarvis.Core.Events.Roles;
using Infrastructure.Caching;
using Microsoft.EntityFrameworkCore;

namespace Jarvis.Core.Controllers
{
    [Authorize]
    [Route("roles")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        [HttpGet]
        [Authorize(nameof(CorePolicy.RolePolicy.Role_Read))]
        public async Task<IActionResult> GetAsync(
            [FromQuery] Paging paging,
            [FromServices] IWorkContext workContext,
            [FromServices] ICoreUnitOfWork uow)
        {
            var context = await workContext.GetContextAsync(nameof(CorePolicy.RolePolicy.Role_Read));

            var repoRole = uow.GetRepository<IRoleRepository>();
            var paged = await repoRole.PagingAsync(context, paging);

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

        [HttpGet("{key}")]
        [Authorize(nameof(CorePolicy.RolePolicy.Role_Read))]
        public async Task<IActionResult> GetAsync(
            [FromRoute] Guid key,
            [FromServices] IWorkContext workContext,
            [FromServices] ICoreUnitOfWork uow)
        {
            var context = await workContext.GetContextAsync(nameof(CorePolicy.RolePolicy.Role_Read));

            var repoRole = uow.GetRepository<IRoleRepository>();
            var role = await repoRole.GetRoleByKeyAsync(context, key);
            if (role == null)
                return NotFound();

            return Ok((RoleModel)role);
        }

        [HttpPost]
        [Authorize(nameof(CorePolicy.RolePolicy.Role_Create))]
        public async Task<IActionResult> PostAsync(
            [FromBody] RoleModel model,
            [FromServices] IWorkContext workContext,
            [FromServices] ICoreUnitOfWork uow,
            [FromServices] IEventFactory eventFactory)
        {
            var tenantCode = await workContext.GetTenantKeyAsync();
            var repoRole = uow.GetRepository<IRoleRepository>();

            var roleId = Guid.NewGuid();
            var roleKey = Guid.NewGuid();
            await repoRole.InsertAsync(new Role
            {
                CreatedAt = DateTime.Now,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = workContext.GetUserKey(),
                Id = roleId,
                Key = roleKey,
                TenantCode = tenantCode,
                Name = model.Name,
            });

            var repoRoleClaim = uow.GetRepository<IPermissionRepository>();
            await repoRoleClaim.InsertRoleClaimsAsync(new List<IdentityRoleClaim<Guid>> {
                new IdentityRoleClaim<Guid>{
                    RoleId = roleId,
                    ClaimType = RoleClaimType.Function.ToString(),
                    ClaimValue = string.Join('|', model.FunctionClaims)
                }
            });

            await uow.CommitAsync();

            eventFactory.GetOrAddEvent<IEvent<RoleCreatedEventModel>, IRoleCreatedEvent>().ForEach(async (e) =>
            {
                await e.PublishAsync(new RoleCreatedEventModel
                {
                    TenantKey = tenantCode,
                    RoleKey = roleKey,
                    Name = model.Name
                });
            });
            return Ok();
        }

        [HttpPut("{key}")]
        [Authorize(nameof(CorePolicy.RolePolicy.Role_Update))]
        public async Task<IActionResult> PutAsync(
            [FromRoute] Guid key,
            [FromBody] RoleModel model,
            [FromServices] IWorkContext workContext,
            [FromServices] ICoreUnitOfWork uow,
            [FromServices] IEventFactory eventFactory,
            [FromServices] ICacheService cacheService)
        {
            var tenantCode = await workContext.GetTenantKeyAsync();

            var repoRole = uow.GetRepository<IRoleRepository>();
            var role = await repoRole.GetRoleByKeyAsync(tenantCode, key);
            if (role == null)
                return NotFound();

            //Update role
            role.Name = model.Name;
            role.UpdatedAt = DateTime.Now;
            role.UpdatedAtUtc = DateTime.UtcNow;
            role.UpdatedBy = workContext.GetUserKey();

            repoRole.Update(role);

            //Update claim
            var repoPermission = uow.GetRepository<IPermissionRepository>();

            var roleClaims = await repoPermission.FindRoleClaimByRoleAsync(role.Id);
            foreach (var item in roleClaims)
            {
                if (item.ClaimType == RoleClaimType.Function.ToString())
                    item.ClaimValue = string.Join('|', model.FunctionClaims);

                if (item.ClaimType == RoleClaimType.Data.ToString())
                    item.ClaimValue = string.Join('|', model.DataClaims);
            }
            await uow.CommitAsync();

            // Xoá token còn hạn sử dụng
            await DeleteTokenAsync(uow, cacheService, role.Id);

            //Notification
            eventFactory.GetOrAddEvent<IEvent<RoleUpdatedEventModel>, IRoleUpdatedEvent>().ForEach(async (e) =>
            {
                await e.PublishAsync(new RoleUpdatedEventModel
                {
                    TenantCode = tenantCode,
                    IdRole = key,
                    Name = model.Name
                });
            });
            return Ok();
        }

        [HttpDelete("{key}")]
        [Authorize(nameof(CorePolicy.RolePolicy.Role_Delete))]
        public async Task<IActionResult> DeleteAsync(
            [FromRoute] Guid key,
            [FromServices] IWorkContext workContext,
            [FromServices] ICoreUnitOfWork uow,
            [FromServices] IEventFactory eventFactory,
            [FromServices] ICacheService cacheService)
        {
            var tenantCode = await workContext.GetTenantKeyAsync();
            var repoRole = uow.GetRepository<IRoleRepository>();
            var role = await repoRole.GetRoleByKeyAsync(tenantCode, key);
            if (role == null)
                return NotFound("Dữ liệu không tồn tại");

            repoRole.Delete(role);

            var repoPermission = uow.GetRepository<IPermissionRepository>();
            var claims = await repoPermission.FindRoleClaimByRoleAsync(role.Id);
            repoPermission.DeleteRoleClaims(claims);

            await uow.CommitAsync();

            //xóa token của các tk dùng quyền này
            await DeleteTokenAsync(uow, cacheService, role.Id);

            eventFactory.GetOrAddEvent<IEvent<RoleDeletedEventModel>, IRoleDeletedEvent>().ForEach(async (e) =>
            {
                await e.PublishAsync(new RoleDeletedEventModel
                {
                    TenantCode = tenantCode,
                    IdRole = key
                });
            });
            return Ok();
        }


        [HttpGet("claims/{key?}")]
        [Authorize(nameof(CorePolicy.RolePolicy.Role_Read))]
        public async Task<IActionResult> GetClaimsAsync(
            [FromRoute] Guid key,
            [FromServices] ICoreUnitOfWork uow,
            [FromServices] IPoliciesStorage policiesStorage)
        {
            var policies = policiesStorage.GetPolicies();
            if (key == Guid.Empty)
            {
                return Ok(policies.Select(x => new ClaimModel
                {
                    Code = x.Code,
                    Name = x.Name,
                    GroupCode = x.GroupCode,
                    GroupName = x.GroupName,
                    ModuleCode = x.ModuleCode,
                    ModuleName = x.ModuleName
                }).ToList());
            }

            var repoRole = uow.GetRepository<IRoleRepository>();
            var role = await repoRole.GetQuery().FirstOrDefaultAsync(x => x.Key == key);
            if (role == null)
                return NotFound("Vai trò không tồn tại");

            var repoUserRole = uow.GetRepository<IPermissionRepository>();
            var roleClaims = await repoUserRole.FindRoleClaimByRoleAsync(role.Id);

            var fClaims = new List<string>();
            var functionClaims = roleClaims.FirstOrDefault(x => x.ClaimType == RoleClaimType.Function.ToString());
            if (functionClaims != null)
                fClaims = string.IsNullOrEmpty(functionClaims.ClaimValue) ? null : functionClaims.ClaimValue.Split('|').ToList();

            var claims = new List<ClaimModel>();
            foreach (var policy in policies)
            {
                var claim = new ClaimModel();
                claim.Code = policy.Code;
                claim.Name = policy.Name;
                claim.GroupCode = policy.GroupCode;
                claim.GroupName = policy.GroupName;
                claim.ModuleCode = policy.ModuleCode;
                claim.ModuleName = policy.ModuleName;

                if (fClaims.FirstOrDefault(x => x == policy.Code) != null)
                    claim.Selected = true;
                else
                    claim.Selected = false;

                claims.Add(claim);
            }

            return Ok(claims);
        }

        private async Task DeleteTokenAsync(ICoreUnitOfWork uow, ICacheService cacheService, Guid idRole)
        {
            //lấy các tài khoản dùng quyền này
            var repoUserRole = uow.GetRepository<IUserRepository>();
            var identityUserRoles = await repoUserRole.FindByIdRoleAsync(idRole);

            if (!identityUserRoles.Any())
                return;

            var idUsers = identityUserRoles.Select(x => x.UserId).ToList();

            //lấy các token của các tài khoản
            var repoTokenInfo = uow.GetRepository<ITokenInfoRepository>();
            var tokens = await repoTokenInfo.QueryByUserAsync(idUsers);

            foreach (var item in tokens)
            {
                repoTokenInfo.Delete(item);

                //xóa token trong cache
                await cacheService.RemoveAsync($":TokenInfos:{item.Key}");
            }

            await uow.CommitAsync();
        }
    }
}
