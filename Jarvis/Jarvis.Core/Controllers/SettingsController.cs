using Jarvis.Core.Constants;
using Jarvis.Core.Database;
using Jarvis.Core.Database.Poco;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Jarvis.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Jarvis.Core.Permissions;
using Jarvis.Core.Database.Repositories;
using Jarvis.Core.Services;
using System.Threading.Tasks;
using Infrastructure.Extensions;
using Infrastructure.Abstractions.Events;
using Jarvis.Core.Models.Events.Settings;
using Jarvis.Core.Events.Settings;

namespace Jarvis.Core.Controllers
{
    [Authorize]
    [Route("settings")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        [HttpGet]
        [Authorize(nameof(CorePolicy.SettingPolicy.Setting_Read))]
        public async Task<IActionResult> GetAsync(
            [FromServices] ICoreUnitOfWork uow,
            [FromServices] IWorkContext workContext,
            [FromServices] ISettingService settingService
        )
        {
            //lấy ra tất các các group default trong Db có tenantCode = Guid.Empty() hiển thị
            // setting nào có tenantCode != Guid.Empty rồi thì lấy dữ liệu db ra
            var tenantCode = workContext.GetTenantKey();

            var repo = uow.GetRepository<ISettingRepository>();
            var settings = await repo.GetByTenantCodesAsync(new List<Guid> { Guid.Empty, tenantCode });

            var grouped = settings.GroupBy(x => x.Group);
            var groupNames = settingService.GetGroupSettings();

            var groups = new List<SettingGroupModel>();
            foreach (var element in grouped)
            {
                var group = new SettingGroupModel
                {
                    Key = element.Key,
                    Name = groupNames.ContainsKey(element.Key) ? groupNames[element.Key] : "",
                    Settings = new List<SettingModel>()
                };

                var defaultSettings = element.Where(x => x.TenantCode == Guid.Empty);
                var tenantSettings = element.Where(x => x.TenantCode == tenantCode);

                foreach (var item in defaultSettings)
                {
                    var setting = new SettingModel
                    {
                        TenantCode = item.TenantCode,
                        Key = item.Key,
                        Code = item.Code,
                        Name = item.Name,
                        Description = item.Description,
                        Note = item.Note,
                        Type = EnumExtension.ToEnum<SettingType>(item.Type),
                        Group = item.Group,
                        Options = string.IsNullOrWhiteSpace(item.Options) ? new Dictionary<string, string>() : item.Options.Split("|").Select(x => x.Split(":")).ToDictionary(x => x[0], x => x[1]),
                        Value = item.Value,
                    };

                    var tenantSetting = tenantSettings.FirstOrDefault(x => x.Code == item.Code);
                    if (tenantSetting != null)
                    {
                        setting.Key = tenantSetting.Key;
                        setting.TenantCode = tenantSetting.TenantCode;
                        setting.Value = tenantSetting.Value;
                    }

                    group.Settings.Add(setting);
                }

                groups.Add(group);
            }

            return Ok(groups);
        }

        [HttpGet("group/{key}")]
        [Authorize(nameof(CorePolicy.SettingPolicy.Setting_Read))]
        public async Task<IActionResult> GetGroupAsync(
            [FromRoute] string key,
            [FromServices] ICoreUnitOfWork uow,
            [FromServices] IWorkContext workContext)
        {
            var tenantCode = await workContext.GetTenantKeyAsync();

            var settingRepo = uow.GetRepository<ISettingRepository>();
            var settings = await settingRepo.GetByGroupAsync(tenantCode, key);
            return Ok(settings.Select(x => new
            {
                x.Id,
                x.Code,
                x.Value
            }));
        }

        [HttpGet("{key}")]
        [Authorize(nameof(CorePolicy.SettingPolicy.Setting_Read))]
        public async Task<IActionResult> GetSettingAsync(
            [FromRoute] string key,
            [FromServices] ICoreUnitOfWork uow,
            [FromServices] IWorkContext workContext)
        {
            var tenantCode = await workContext.GetTenantKeyAsync();

            var settingRepo = uow.GetRepository<ISettingRepository>();
            var setting = await settingRepo.GetByKeyAsync(tenantCode, key);
            return Ok(new
            {
                setting.Id,
                setting.Code,
                setting.Value
            });
        }

        [AllowAnonymous]
        [HttpGet("share")]
        public async Task<IActionResult> GetShareByKeyAsync(
            [FromQuery] SettingKey key,
            [FromServices] ICoreUnitOfWork uow)
        {
            var repo = uow.GetRepository<ISettingRepository>();
            var setting = await repo.GetByKeyAsync(key.ToString());
            return Ok(setting);
        }

        [HttpPost("{group}")]
        [Authorize(nameof(CorePolicy.SettingPolicy.Setting_Update))]
        public async Task<IActionResult> PostAsync(
            [FromRoute] string group,
            [FromBody] List<SettingModel> input,
            [FromServices] ICoreUnitOfWork uow,
            [FromServices] IWorkContext workContext,
            [FromServices] ISettingService settingService,
            [FromServices] IEventFactory eventFactory)
        {
            var tenantCode = workContext.GetTenantKey();
            var userCode = workContext.GetUserKey();

            var repo = uow.GetRepository<ISettingRepository>();
            var settings = await repo.GetByGroupTenantAsync(tenantCode, group);
            var indexSetting = settings.ToDictionary(x => x.Code, x => x);
            var defaultSettings = settingService.GetDefaultSettings().Where(x => x.Group == group);

            foreach (var item in input)
            {
                if (indexSetting.ContainsKey(item.Code))
                {
                    var setting = indexSetting[item.Code];
                    setting.Value = item.Value;
                    setting.UpdatedAt = DateTime.Now;
                    setting.UpdatedAtUtc = DateTime.UtcNow;
                    setting.UpdatedBy = userCode;
                }
                else
                {
                    var defaultSetting = defaultSettings.FirstOrDefault(x => x.Code == item.Code);
                    await repo.InsertAsync(new Setting
                    {
                        Key = Guid.NewGuid(),
                        Code = item.Code,
                        Name = defaultSetting.Name,
                        Group = group,
                        Value = item.Value,
                        Options = defaultSetting.Options,
                        Type = defaultSetting.Type,
                        CreatedAt = DateTime.Now,
                        CreatedAtUtc = DateTime.UtcNow,
                        CreatedBy = userCode,
                        Description = defaultSetting.Description,
                        Note = defaultSetting.Note,
                        TenantCode = tenantCode
                    });
                }
            }

            await uow.CommitAsync();

            eventFactory.GetOrAddEvent<IEvent<SettingUpdatedEventModel>, ISettingUpdatedEvent>().ForEach(async (e) =>
            {
                await e.PublishAsync(new SettingUpdatedEventModel
                {
                    TenantKey = tenantCode,
                    UserKey = userCode,
                    Group = group
                });
            });
            return Ok();
        }
    }
}
