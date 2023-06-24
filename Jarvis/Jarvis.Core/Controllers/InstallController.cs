using Infrastructure.Database.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Jarvis.Core.Constants;
using Jarvis.Core.Database;
using Jarvis.Core.Database.Poco;
using Jarvis.Core.Database.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;
using Jarvis.Core.Models.Install;
using Jarvis.Core.Services;

namespace Jarvis.Core.Controllers
{
    [Route("install")]
    [ApiController]
    public class InstallController : ControllerBase
    {
        /// <summary>
        /// Step 1: Tạo doanh nghiệp tổng
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("tenant")]
        public async Task<IActionResult> TenantAsync(
            [FromBody] InstallTenantModel model,
            [FromServices] ICoreUnitOfWork uow)
        {
            var repoTenant = uow.GetRepository<ITenantRepository>();
            if (await repoTenant.AnyAsync())
                return BadRequest("Hệ thống đã được cài đặt, không thể cài lại");

            var tenantKey = Guid.NewGuid();

            //Thông tin doanh nghiệp
            await repoTenant.InsertAsync(new Tenant
            {
                Name = model.TaxCode,
                Key = tenantKey,
                IsEnable = true,
                CreatedAt = DateTime.Now,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = Guid.Empty,
                Path = tenantKey.ToString()
            });

            await repoTenant.InsertInfoAsync(new TenantInfo
            {
                TaxCode = model.TaxCode,
                Address = model.Address,
                City = model.City,
                Country = model.Country,
                District = model.District,
                FullNameVi = model.FullNameVi,
                FullNameEn = model.FullNameEn,
                Key = tenantKey,
                IsCurrent = true
            });

            var splited = model.HostName.Split(';');
            foreach (var item in splited)
            {
                await repoTenant.InsertHostAsync(new TenantHost
                {
                    Key = tenantKey,
                    HostName = model.HostName
                });
            }

            await uow.CommitAsync();
            return Ok(tenantKey);
        }

        /// <summary>
        /// Step 2: Tạo tài khoản root đăng nhập doanh nghiệp tổng
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("user")]
        public async Task<IActionResult> UserAsync(
            [FromBody] InstallUserModel model,
            [FromServices] UserManager<User> userManager,
            [FromServices] ICoreUnitOfWork uow)
        {
            // Tạo tài khoản admin
            var userKey = Guid.NewGuid();
            var user = new User
            {
                CreatedAt = DateTime.Now,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = Guid.Empty,
                UserName = model.UserName,
                TenantCode = model.TenantCode,
                Type = UserType.SuperAdmin.GetHashCode(),
                Key = userKey,
                Id = Guid.NewGuid()
            };
            var identityResult = await userManager.CreateAsync(user, model.Password);
            if (!identityResult.Succeeded)
                return BadRequest(string.Join(";", identityResult.Errors.Select(x => x.Description)));

            var repoUser = uow.GetRepository<IUserRepository>();
            await repoUser.InsertUserInfoAsync(new UserInfo
            {
                AvatarPath = null,
                FullName = model.FullName,
                Key = userKey
            });
            await uow.CommitAsync();
            return Ok();
        }

        /// <summary>
        /// Step 3: Tạo setting mặc định
        /// </summary>
        /// <returns></returns>
        [HttpPost("setting")]
        public async Task<IActionResult> SettingAsync(
            [FromServices] ISettingService settingService,
            [FromServices] ICoreUnitOfWork uow
        )
        {
            //kiểm tra db có setting nào chưa. có rồi thì không cho thêm nữa
            var repoSetting = uow.GetRepository<ISettingRepository>();
            if (await repoSetting.AnyAsync())
                return BadRequest("Hệ thống đã được cài đặt setting mặc định, không thể cài lại");

            //thêm vào database các setting có tenantCode = Guid.Empty()
            var defaultDatas = settingService.GetDefaultSettings();
            var defaultGroups = defaultDatas.GroupBy(x => x.Group);

            var groupSettingKeys = settingService.GetGroupSettings();

            //lấy dữ liệu mặc định và insert vào db
            foreach (var item in defaultDatas)
            {
                await repoSetting.InsertAsync(new Setting
                {
                    Key = Guid.NewGuid(),
                    Group = item.Group,
                    Code = item.Code,
                    Name = item.Name,
                    Value = item.Value,
                    Options = item.Options,
                    Type = item.Type,
                    CreatedAt = DateTime.Now,
                    CreatedAtUtc = DateTime.UtcNow,
                    CreatedBy = Guid.Empty,
                    Description = item.Description,
                    TenantCode = Guid.Empty,
                });
            }
            await uow.CommitAsync();

            return Ok();
        }
    }
}
