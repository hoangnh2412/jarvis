using Infrastructure.Database.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Jarvis.Core.Constants;
using Jarvis.Core.Database;
using Jarvis.Core.Database.Poco;
using Jarvis.Core.Database.Repositories;
using Jarvis.Core.Permissions;
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
        private readonly ICoreUnitOfWork _uow;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly ISettingService _settingService;

        public InstallController(
            ICoreUnitOfWork uow,
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            ISettingService settingService)
        {
            _uow = uow;
            _userManager = userManager;
            _roleManager = roleManager;
            _settingService = settingService;
        }

        /// <summary>
        /// Step 1: Tạo doanh nghiệp tổng
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("tenant")]
        public async Task<IActionResult> TenantAsync([FromBody]InstallTenantModel model)
        {
            var repoTenant = _uow.GetRepository<ITenantRepository>();
            if (await repoTenant.AnyAsync())
                return BadRequest("Hệ thống đã được cài đặt, không thể cài lại");

            var tenantCode = Guid.NewGuid();

            //Thông tin doanh nghiệp
            await repoTenant.InsertAsync(new Tenant
            {
                Name = model.TaxCode,
                Code = tenantCode,
                IsEnable = true,
                CreatedAt = DateTime.Now,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = Guid.Empty,
                Path = tenantCode.ToString()
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
                Code = tenantCode,
                IsCurrent = true
            });

            var splited = model.HostName.Split(';');
            foreach (var item in splited)
            {
                await repoTenant.InsertHostAsync(new TenantHost
                {
                    Code = tenantCode,
                    HostName = model.HostName
                });
            }

            await _uow.CommitAsync();
            return Ok(tenantCode);
        }

        /// <summary>
        /// Step 2: Tạo tài khoản root đăng nhập doanh nghiệp tổng
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("user")]
        public async Task<IActionResult> UserAsync([FromBody]InstallUserModel model)
        {
            //Tạo tài khoản ROOT
            var idUser = Guid.NewGuid();
            var user = new User
            {
                CreatedAt = DateTime.Now,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = Guid.Empty,
                UserName = model.UserName,
                TenantCode = model.TenantCode,
                Id = idUser
            };
            var identityResult = await _userManager.CreateAsync(user, model.Password);
            if (!identityResult.Succeeded)
                return BadRequest(string.Join(";", identityResult.Errors.Select(x => x.Description)));

            var repoUserInfo = _uow.GetRepository<IUserRepository>();
            await repoUserInfo.InsertUserInfoAsync(new UserInfo
            {
                AvatarPath = null,
                FullName = model.FullName,
                Id = idUser
            });
            await _uow.CommitAsync();

            //Phân quyền tài khoản root
            identityResult = await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim(nameof(SpecialPolicy.Special_DoEnything), $"{default(ClaimOfResource)}|{default(ClaimOfChildResource)}"));
            if (!identityResult.Succeeded)
                return BadRequest(string.Join(";", identityResult.Errors.Select(x => x.Description)));
            return Ok();
        }

        /// <summary>
        /// Step 3: Tạo setting mặc định
        /// </summary>
        /// <returns></returns>
        [HttpPost("setting")]
        public async Task<IActionResult> SettingAsync()
        {
            //kiểm tra db có setting nào chưa. có rồi thì không cho thêm nữa
            var repoSetting = _uow.GetRepository<ISettingRepository>();
            if (await repoSetting.AnyAsync())
                return BadRequest("Hệ thống đã được cài đặt setting mặc định, không thể cài lại");

            //thêm vào database các setting có tenantCode = Guid.Empty()
            var defaultDatas = _settingService.GetDefaultSettings();
            var defaultGroups = defaultDatas.GroupBy(x => x.Group);

            var groupSettingKeys = _settingService.GetGroupSettings();

            //lấy dữ liệu mặc định và insert vào db
            foreach (var item in defaultDatas)
            {
                await repoSetting.InsertAsync(new Setting
                {
                    Code = Guid.NewGuid(),
                    Group = item.Group,
                    Key = item.Key,
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

            await _uow.CommitAsync();

            return Ok("Thêm setting mặc định thành công");
        }



        //[HttpPost("setting")]
        //public async Task<IActionResult> SettingAsync([FromBody]InstallSettingModel model)
        //{
        //    var settings = new List<Setting>();
        //    foreach (var item in DefaultData.GeneralSettings)
        //    {
        //        settings.Add(new Setting
        //        {
        //            Code = Guid.NewGuid(),
        //            TenantCode = model.TenantCode,
        //            CreatedAt = DateTime.Now,
        //            CreatedAtUtc = DateTime.UtcNow,
        //            CreatedBy = model.IdUser,
        //            Group = item.Group,
        //            Key = item.Key,
        //            Name = item.Name,
        //            Value = item.Value,
        //            Options = item.Options,
        //            Type = item.Type
        //        });
        //    }
        //    var repoSetting = _uow.GetRepository<ISettingRepository>();
        //    await repoSetting.InsertsAsync(settings);
        //    await _uow.CommitAsync();
        //    return Ok();
        //}
    }
}
