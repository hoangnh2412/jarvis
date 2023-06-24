using Jarvis.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.AspNetCore.Authorization;
using Jarvis.Core.Permissions;
using Infrastructure.Database.Models;
using Jarvis.Core.Database;
using Jarvis.Core.Database.Poco;
using System.Linq;
using System.Collections.Generic;
using Jarvis.Core.Database.Repositories;
using Jarvis.Core.Constants;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Infrastructure.Database.Entities;
using Jarvis.Core.Models.Tenant;
using Jarvis.Core.Extensions;
using Infrastructure.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using Infrastructure.Abstractions;
using Infrastructure;
using Infrastructure.Abstractions.Events;
using Jarvis.Core.Models.Events.Tenants;
using Jarvis.Core.Events.Tenants;
using Infrastructure.Caching;

namespace Jarvis.Core.Controllers
{
    //TODO: Cần tách job update metadata nếu update setting
    [Authorize]
    [Route("tenants")]
    [ApiController]
    public class TenantsController : ControllerBase
    {
        private readonly ICoreUnitOfWork _uow;

        public TenantsController(ICoreUnitOfWork uow)
        {
            _uow = uow;
        }

        [HttpGet]
        [Authorize(nameof(CorePolicy.TenantPolicy.Tenant_Read))]
        public async Task<IActionResult> GetAsync(
            [FromQuery] Paging paging,
            [FromServices] IWorkContext workContext)
        {
            var idTenant = await workContext.GetTenantKeyAsync();
            var repoTenant = _uow.GetRepository<ITenantRepository>();
            var paged = await repoTenant.PagingAsync(idTenant, paging);
            var tenantCodes = paged.Data.Select(x => x.Key).ToList();
            var infos = (await repoTenant.GetInfoByCodesAsync(tenantCodes)).ToDictionary(x => x.Key, x => x);
            var hosts = (await repoTenant.GetHostByCodesAsync(tenantCodes)).GroupBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToList());

            var result = new Paged<TenantModel>
            {
                Page = paged.Page,
                Q = paged.Q,
                Size = paged.Size,
                TotalItems = paged.TotalItems,
                TotalPages = paged.TotalPages,
            };

            var data = new List<TenantModel>();
            foreach (var item in paged.Data)
            {
                var model = new TenantModel();
                model.Code = item.Key;
                model.HostName = string.Join(';', hosts[item.Key].Select(x => x.HostName));
                model.IsEnable = item.IsEnable;
                model.Theme = item.Theme;

                if (!infos.ContainsKey(item.Key))
                {
                    data.Add(model);
                    continue;
                }

                var info = infos[item.Key];
                model.Info = new TenantInfoModel
                {
                    Id = info.Id,
                    Code = info.Key,
                    Address = info.Address,
                    City = info.City,
                    Country = info.Country,
                    District = info.District,
                    Emails = info.Emails,
                    Fax = info.Fax,
                    FullNameVi = info.FullNameVi,
                    FullNameEn = info.FullNameEn,
                    Phones = info.Phones,
                    TaxCode = info.TaxCode,
                };
                data.Add(model);
            }
            result.Data = data;
            return Ok(result);
        }

        [HttpGet("{code}")]
        [Authorize(nameof(CorePolicy.TenantPolicy.Tenant_Read))]
        public async Task<IActionResult> GetAsync(
            [FromRoute] Guid code)
        {
            var repoTenant = _uow.GetRepository<ITenantRepository>();
            var tenant = await repoTenant.GetByCodeAsync(code);
            if (tenant == null)
                return NotFound();

            var info = await repoTenant.GetInfoByCodeAsync(code);
            if (info == null)
                return NotFound();

            var hosts = await repoTenant.GetHostByCodeAsync(code);
            if (hosts.Count == 0)
                return NotFound();

            var model = new TenantModel
            {
                Code = tenant.Key,
                HostName = string.Join(';', hosts.Select(x => x.HostName)),
                Info = new TenantInfoModel
                {
                    TaxCode = info.TaxCode,
                    FullNameVi = info.FullNameVi,
                    Address = info.Address,
                }
            };

            return Ok(model);
        }

        [HttpPost]
        [Authorize(nameof(CorePolicy.TenantPolicy.Tenant_Create))]
        public async Task<IActionResult> PostAsync(
            [FromBody] CreateTenantCommand input,
            [FromServices] IWorkContext workContext,
            [FromServices] IEventFactory eventFactory,
            [FromServices] IModuleManager moduleManager,
            [FromServices] UserManager<User> userManager,
            [FromServices] IPasswordValidator<User> passwordValidator,
            [FromServices] IPasswordHasher<User> passwordHasher)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = workContext.GetUserKey();
            var tenant = await workContext.GetCurrentTenantAsync();

            var repo = _uow.GetRepository<ITenantRepository>();
            if (await repo.AnyByNameAsync(input.Info.TaxCode))
                throw new Exception("Mã số thuế đã bị trùng");

            if (await repo.AnyByHostNameAsync(input.HostName))
                throw new Exception("Tên miền đã bị trùng");

            var tenantId = await InsertTenant(input, userId, tenant);

            //Nếu ko nhập pasword sẽ tự động random
            if (string.IsNullOrEmpty(input.User.Password))
                input.User.Password = RandomExtension.Random(10);
            await InsertAdmin(userManager, passwordValidator, passwordHasher, input, userId, tenantId);

            eventFactory.GetOrAddEvent<IEvent<TenantCreatedEventModel>, ITenantCreatedEvent>().ForEach(async (e) =>
            {
                await e.PublishAsync(new TenantCreatedEventModel
                {
                    TenantId = tenant.Key,
                    Password = input.User.Password,
                });
            });

            return Ok(new
            {
                TenantCode = tenantId,
            });
        }

        [HttpPut("{code}")]
        [Authorize(nameof(CorePolicy.TenantPolicy.Tenant_Update))]
        public async Task<IActionResult> PutAsync(
            [FromRoute] Guid code,
            [FromBody] UpdateTenantCommand model,
            [FromServices] IWorkContext workContext,
            [FromServices] IEventFactory eventFactory,
            [FromServices] ICacheService cacheService)
        {
            var userCode = workContext.GetUserKey();
            var repoTenant = _uow.GetRepository<ITenantRepository>();
            var tenant = await repoTenant.GetByCodeAsync(code);
            if (tenant == null)
                return NotFound();

            if (tenant.IsEnable)
                throw new Exception("Chi nhánh đã tạo đăng ký phát hành không thể sửa!");

            var info = await repoTenant.GetInfoByCodeAsync(code);
            if (info == null)
                return NotFound();

            if ((await repoTenant.QueryHostByHostNameAsync(model.HostName)).Any(x => x.Key != code))
                throw new Exception("Tên miền đã bị trùng");

            var hosts = await repoTenant.GetHostByCodeAsync(code);
            if (hosts.Count == 0)
                return NotFound();

            var news = model.HostName.Split(';');

            var deletes = new List<TenantHost>();
            foreach (var item in hosts)
            {
                var delete = news.FirstOrDefault(x => x == item.HostName);
                if (delete == null)
                    deletes.Add(item);
            }

            var inserts = new List<TenantHost>();
            foreach (var item in news)
            {
                var insert = hosts.FirstOrDefault(x => x.HostName == item);
                if (insert == null)
                    inserts.Add(new TenantHost
                    {
                        Key = code,
                        HostName = item
                    });
            }

            if (deletes.Count != 0)
            {
                repoTenant.DeleteHosts(deletes);

                //xóa cache
                foreach (var item in deletes)
                {
                    await cacheService.RemoveAsync($":TenantHost:{item.HostName}");
                }

                //xóa token của các tk trong chi nhánh này
                await DeleteTokenAsync(cacheService, code);
            }

            if (inserts.Count != 0)
                await repoTenant.InsertHostsAsync(inserts);

            repoTenant.UpdateTenantFields(tenant,
                tenant.Set(x => x.Name, model.Info.TaxCode),
                tenant.Set(x => x.UpdatedAt, DateTime.Now),
                tenant.Set(x => x.UpdatedAtUtc, DateTime.UtcNow),
                tenant.Set(x => x.UpdatedBy, userCode));

            repoTenant.UpdateInfoFields(info,
                info.Set(x => x.TaxCode, model.Info.TaxCode),
                info.Set(x => x.FullNameVi, model.Info.FullNameVi),
                info.Set(x => x.Address, model.Info.Address));

            await _uow.CommitAsync();

            eventFactory.GetOrAddEvent<IEvent<TenantUpdatedEventModel>, ITenantUpdatedEvent>().ForEach(async (e) =>
            {
                await e.PublishAsync(new TenantUpdatedEventModel
                {
                    TenantCode = tenant.Key
                });
            });
            return Ok();
        }

        [HttpDelete("{code}")]
        [Authorize(nameof(CorePolicy.TenantPolicy.Tenant_Delete))]
        public async Task<IActionResult> DeleteAsync(
            [FromRoute] Guid code,
            [FromServices] IWorkContext workContext,
            [FromServices] IEventFactory eventFactory,
            [FromServices] ICacheService cacheService)
        {
            var userCode = workContext.GetUserKey();
            var repoTenant = _uow.GetRepository<ITenantRepository>();
            var tenant = await repoTenant.GetByCodeAsync(code);
            if (tenant == null)
                return NotFound();

            if (tenant.IsEnable)
                throw new Exception("Chi nhánh đã tạo đăng ký phát hành không thể xóa!");

            var tenantHosts = await repoTenant.GetHostByCodeAsync(code);
            if (!tenantHosts.Any())
                return NotFound();

            repoTenant.UpdateTenantFields(tenant,
                tenant.Set(x => x.DeletedVersion, tenant.Id),
                tenant.Set(x => x.DeletedAt, DateTime.Now),
                tenant.Set(x => x.DeletedAtUtc, DateTime.UtcNow),
                tenant.Set(x => x.DeletedBy, userCode));

            foreach (var tenantHost in tenantHosts)
            {
                repoTenant.UpdateTenantHostFields(tenantHost,
                    tenantHost.Set(x => x.DeletedVersion, tenantHost.Id));

                //xóa cache
                await cacheService.RemoveAsync($":TenantHost:{tenantHost.HostName}");
            }

            //xóa token của các tk trong chi nhánh này
            await DeleteTokenAsync(cacheService, code);

            await _uow.CommitAsync();

            eventFactory.GetOrAddEvent<IEvent<TenantDeletedEventModel>, ITenantDeletedEvent>().ForEach(async (e) =>
            {
                await e.PublishAsync(new TenantDeletedEventModel
                {
                    TenantCode = tenant.Key
                });
            });
            return Ok();
        }

        /// <summary>
        /// xóa token của các tài khoản trong 1 công ty/chi nhánh
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private async Task DeleteTokenAsync(ICacheService cacheService, Guid code)
        {
            var repoTokenInfo = _uow.GetRepository<ITokenInfoRepository>();
            var tokens = await repoTokenInfo.QueryByTenantAsync(code);

            foreach (var token in tokens)
            {
                repoTokenInfo.Delete(token);

                //xóa token trong cache
                await cacheService.RemoveAsync($":TokenInfos:{token.Key}");
            }
        }

        /// <summary>
        /// thêm tk admin của chi nhánh và gán với 1 nhóm quyền mặc định
        /// </summary>
        /// <param name="input"></param>
        /// <param name="userId"></param>
        /// <param name="tenantId"></param>
        /// <param name="idRole"></param>
        /// <returns></returns>
        private async Task InsertAdmin(UserManager<User> userManager, IPasswordValidator<User> passwordValidator, IPasswordHasher<User> passwordHasher, CreateTenantCommand input, Guid userId, Guid tenantId)
        {
            var repo = _uow.GetRepository<IUserRepository>();

            var user = new User
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = userId,
                UserName = input.User.UserName,
                NormalizedUserName = input.User.UserName.ToUpper(),
                SecurityStamp = Guid.NewGuid().ToString().ToUpper().Replace("-", ""),
                LockoutEnabled = true,
                TenantCode = tenantId,
                Email = input.User.Email,
                NormalizedEmail = string.IsNullOrEmpty(input.User.Email) ? null : input.User.Email.ToUpper(),
                Type = UserType.Admin.GetHashCode()
            };

            var validateResult = await passwordValidator.ValidateAsync(userManager, user, input.User.Password);
            if (!validateResult.Succeeded)
                throw new Exception(validateResult.ToMessage());

            var passwordHashed = passwordHasher.HashPassword(user, input.User.Password);
            user.PasswordHash = passwordHashed;

            await repo.InsertUserAsync(user);
            await repo.InsertUserInfoAsync(new UserInfo
            {
                Key = user.Key,
                FullName = input.User.FullName
            });
            await _uow.CommitAsync();
        }

        /// <summary>
        /// thêm mới tenant, tenantInfo
        /// </summary>
        /// <param name="input"></param>
        /// <param name="userId"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        private async Task<Guid> InsertTenant(CreateTenantCommand input, Guid userId, Tenant parent)
        {
            var key = Guid.NewGuid();
            var repo = _uow.GetRepository<ITenantRepository>();
            var tenant = new Tenant
            {
                Key = key,
                Name = input.Info.TaxCode,
                CreatedAt = DateTime.Now,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = userId,
                IdParent = parent.Key,
                Path = $"{parent.Path}|{key}",
                IsEnable = false
            };
            await repo.InsertAsync(tenant);

            var tenantInfo = new TenantInfo
            {
                Key = key,
                IsCurrent = true,
                TaxCode = input.Info.TaxCode,
                Address = input.Info.Address,
                FullNameVi = input.Info.FullNameVi,
                City = "#",
                Country = "#",
                District = "#"
            };
            await repo.InsertInfoAsync(tenantInfo);

            var splited = input.HostName.Split(';');
            foreach (var item in splited)
            {
                await repo.InsertHostAsync(new TenantHost
                {
                    Key = key,
                    HostName = item
                });
            }
            await _uow.CommitAsync();

            return key;
        }

    }
}
