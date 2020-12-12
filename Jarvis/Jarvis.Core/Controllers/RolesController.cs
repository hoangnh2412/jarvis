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
using Microsoft.Extensions.Caching.Distributed;
using Infrastructure.Extensions;

namespace Jarvis.Core.Controllers
{
    [Authorize]
    [Route("roles")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IWorkContext _workContext;
        private readonly IPoliciesStorage _policiesStorage;
        private readonly ICoreUnitOfWork _uow;
        private readonly IDistributedCache _cache;

        public RolesController(
            IWorkContext workContext,
            IPoliciesStorage policiesStorage,
            ICoreUnitOfWork uow,
            IDistributedCache cache)
        {
            _workContext = workContext;
            _policiesStorage = policiesStorage;
            _uow = uow;
            _cache = cache;
        }

        [HttpGet]
        [Authorize(nameof(CorePolicy.RolePolicy.Role_Read))]
        public async Task<IActionResult> GetAsync([FromQuery]Paging paging)
        {
            var context = await _workContext.GetContextAsync(nameof(CorePolicy.RolePolicy.Role_Read));

            var repoRole = _uow.GetRepository<IRoleRepository>();
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

        [HttpGet("{id}")]
        [Authorize(nameof(CorePolicy.RolePolicy.Role_Read))]
        public async Task<IActionResult> GetAsync([FromRoute]Guid id)
        {
            var context = await _workContext.GetContextAsync(nameof(CorePolicy.RolePolicy.Role_Read));

            var repoRole = _uow.GetRepository<IRoleRepository>();
            var role = await repoRole.GetRoleByIdAsync(context, id);
            if (role == null)
                return NotFound();

            return Ok((RoleModel)role);
        }

        [HttpPost]
        [Authorize(nameof(CorePolicy.RolePolicy.Role_Create))]
        public async Task<IActionResult> PostAsync([FromBody]RoleModel model)
        {
            var repoRole = _uow.GetRepository<IRoleRepository>();

            //Create role
            var idRole = Guid.NewGuid();
            await repoRole.InsertAsync(new Role
            {
                CreatedAt = DateTime.Now,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = _workContext.GetUserCode(),
                Id = idRole,
                TenantCode = await _workContext.GetTenantCodeAsync(),
                Name = model.Name,
            });

            //Create claim
            var repoRoleClaim = _uow.GetRepository<IPermissionRepository>();
            await repoRoleClaim.InsertRoleClaimsAsync(model.Claims.Select(x => new IdentityRoleClaim<Guid>
            {
                RoleId = idRole,
                ClaimType = x.Key,
                ClaimValue = x.Value
            }).ToList());

            await _uow.CommitAsync();

            return Ok();
        }

        [HttpPut("{id}")]
        [Authorize(nameof(CorePolicy.RolePolicy.Role_Update))]
        public async Task<IActionResult> PutAsync([FromRoute]Guid id, [FromBody]RoleModel model)
        {
            var context = await _workContext.GetContextAsync(nameof(CorePolicy.RolePolicy.Role_Update));

            var repoRole = _uow.GetRepository<IRoleRepository>();
            var role = await repoRole.GetRoleByIdAsync(context, id);
            if (role == null)
                return NotFound();

            //Update role
            role.Name = model.Name;
            role.UpdatedAt = DateTime.Now;
            role.UpdatedAtUtc = DateTime.UtcNow;
            role.UpdatedBy = _workContext.GetUserCode();

            repoRole.Update(role);

            //Update claim
            var repoUserRole = _uow.GetRepository<IPermissionRepository>();
            var roleClaims = await repoUserRole.FindRoleClaimByRoleAsync(id);
            var clientClaims = model.Claims.Keys.ToList();
            var serverClaims = roleClaims.Select(x => x.ClaimType);

            await repoUserRole.InsertRoleClaimsAsync(clientClaims
                .Except(serverClaims)
                .Select(x => new IdentityRoleClaim<Guid>
                {
                    RoleId = id,
                    ClaimType = x,
                    ClaimValue = model.Claims[x]
                })
                .ToList());

            var removes = serverClaims
                .Except(clientClaims)
                .ToList();
            var claims = new List<IdentityRoleClaim<Guid>>();
            foreach (var item in removes)
            {
                var claim = roleClaims.FirstOrDefault(x => x.ClaimType == item);
                claims.Add(claim);
            }
            repoUserRole.DeleteRoleClaims(claims);


            var updates = clientClaims.Intersect(serverClaims).ToList();
            foreach (var item in updates)
            {
                var claim = roleClaims.FirstOrDefault(x => x.ClaimType == item);
                claim.ClaimValue = model.Claims[item];
            }

            //xóa token của các tk dùng quyền này
            if (removes.Any() || updates.Any())
            {
                await DeleteTokenAsync(id);
            }

            await _uow.CommitAsync();
         
            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(nameof(CorePolicy.RolePolicy.Role_Delete))]
        public async Task<IActionResult> DeleteAsync([FromRoute]Guid id)
        {
            var context = await _workContext.GetContextAsync(nameof(CorePolicy.RolePolicy.Role_Delete));

            var repoRole = _uow.GetRepository<IRoleRepository>();
            var role = await repoRole.GetRoleByIdAsync(context, id);
            if (role == null)
                return NotFound("Dữ liệu không tồn tại");

            repoRole.Delete(role);
            await _uow.CommitAsync();

            //xóa token của các tk dùng quyền này
            await DeleteTokenAsync(id);

            return Ok();
        }


        [HttpGet("claims/{id?}")]
        [Authorize(nameof(CorePolicy.RolePolicy.Role_Read))]
        public async Task<IActionResult> GetClaimsAsync([FromRoute]Guid id)
        {
            var policies = _policiesStorage.GetPolicies();
            if (id == Guid.Empty)
            {
                return Ok(policies.Select(x => new ClaimModel
                {
                    Code = x.Code,
                    Name = x.Name,
                    GroupCode = x.GroupCode,
                    GroupName = x.GroupName,
                    ModuleCode = x.ModuleCode,
                    ModuleName = x.ModuleName,
                    Resource = ClaimOfResource.Tenant.ToString(),
                    Resources = x.ClaimOfResource.ToDictionary(y => y.ToString(), y => y.ToDisplayName()),
                    ChildResource = ClaimOfChildResource.None.ToString(),
                    ChildResources = x.ClaimOfChildResources.ToDictionary(y => y.ToString(), y => y.ToDisplayName())
                }).ToList());
            }

            var repoUserRole = _uow.GetRepository<IPermissionRepository>();
            var roleClaims = (await repoUserRole
                .FindRoleClaimByRoleAsync(id))
                .ToDictionary(x => x.ClaimType, x => x.ClaimValue);

            var claims = new List<ClaimModel>();
            foreach (var policy in policies)
            {
                //k có quyền gì thì bỏ qua
                if (policy.ClaimOfChildResources == null || policy.ClaimOfResource == null)
                    continue;

                var claim = new ClaimModel();
                claim.Code = policy.Code;
                claim.Name = policy.Name;
                claim.GroupCode = policy.GroupCode;
                claim.GroupName = policy.GroupName;
                claim.ModuleCode = policy.ModuleCode;
                claim.ModuleName = policy.ModuleName;

                claim.Resources = policy.ClaimOfResource.ToDictionary(x => x.ToString(), x => x.ToDisplayName());
                claim.ChildResources = policy.ClaimOfChildResources.ToDictionary(x => x.ToString(), x => x.ToDisplayName());

                if (roleClaims.ContainsKey(policy.Code))
                {
                    var claimValue = roleClaims[policy.Code];
                    var splited = claimValue.Split('|');
                    if (splited.Length == 1)
                    {
                        claim.Resource = splited[0];
                        claim.ChildResource = ClaimOfChildResource.None.ToString();
                    }
                    else
                    {
                        claim.Resource = splited[0];
                        claim.ChildResource = splited[1];
                    }
                    claim.Selected = true;
                }
                else
                {
                    claim.Resource = ClaimOfResource.Tenant.ToString();
                    claim.ChildResource = ClaimOfChildResource.None.ToString();
                    claim.Selected = false;
                }

                claims.Add(claim);
            }

            return Ok(claims);
        }


        /// <summary>
        /// xóa các token của các tài khoản đc dùng quyền này
        /// </summary>
        /// <param name="idRole"></param>
        private async Task DeleteTokenAsync(Guid idRole)
        {
            //lấy các tài khoản dùng quyền này
            var repoUserRole = _uow.GetRepository<IUserRepository>();
            var identityUserRoles = await repoUserRole.FindByIdRoleAsync(idRole);

            if (!identityUserRoles.Any())
                return;

            var idUsers = identityUserRoles.Select(x => x.UserId).ToList();

            //lấy các token của các tài khoản
            var repoTokenInfo = _uow.GetRepository<ITokenInfoRepository>();
            var tokens = await repoTokenInfo.QueryByUserAsync(idUsers);

            foreach (var item in tokens)
            {
                repoTokenInfo.Delete(item);

                //xóa token trong cache
                await _cache.RemoveAsync($":TokenInfos:{item.Code}");
            }
        }
    }
}
