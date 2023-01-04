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
            var tenantCode = await workContext.GetTenantCodeAsync();

            var repo = uow.GetRepository<ISettingRepository>();
            var settings = await repo.GetByTenantCodesAsync(new List<Guid> { Guid.Empty, tenantCode });
            settings = settings.Where(x => !x.IsReadOnly).ToList(); //chỉ lấy setting nào đc hiển thị

            var defaulSettings = settings.Where(x => x.TenantCode == Guid.Empty);
            var entities = settings.Except(defaulSettings);

            var groupSettingKeys = settingService.GetGroupSettings();

            var settingGroups = new List<SettingGroupModel>();

            var defaultGroups = defaulSettings.GroupBy(x => x.Group);


            //Hiển thị toàn bộ setting theo group
            foreach (var defaultGroup in defaultGroups)
            {
                //Nếu group nào chưa có DisplayName sẽ ko hiển thị ra ngoài
                if (!groupSettingKeys.ContainsKey(defaultGroup.Key))
                    continue;

                var settingGroup = new SettingGroupModel
                {
                    Key = defaultGroup.Key,
                    Name = groupSettingKeys[defaultGroup.Key],
                    Settings = new List<SettingModel>()
                };

                var defaultSettings = defaultGroup.ToList();
                foreach (var defaultSetting in defaultSettings)
                {

                    //Lấy cấu hình mặc định
                    var setting = new SettingModel
                    {
                        Key = defaultSetting.Key,
                        Name = defaultSetting.Name,
                        Type = EnumExtension.ToEnum<SettingType>(defaultSetting.Type),
                        Group = defaultSetting.Group,
                        Options = string.IsNullOrWhiteSpace(defaultSetting.Options) ? new List<KeyValuePair<string, string>>() : defaultSetting.Options.Split("|").Select(x => x.Split(":")).Select(x => new KeyValuePair<string, string>(x[0], x[1])).ToList(),
                        Value = defaultSetting.Value,
                        Description = defaultSetting.Description
                    };

                    //Lấy giá trị trên CSDL
                    var entity = entities.FirstOrDefault(x => x.Key == defaultSetting.Key);
                    if (entity != null)
                    {
                        setting.Id = entity.Id;
                        setting.Code = entity.Code;
                        setting.TenantCode = entity.TenantCode;
                        setting.Value = entity.Value;
                    }
                    settingGroup.Settings.Add(setting);
                }

                settingGroups.Add(settingGroup);
            }

            return Ok(settingGroups);
        }

        [HttpGet("group/{key}")]
        [Authorize(nameof(CorePolicy.SettingPolicy.Setting_Read))]
        public async Task<IActionResult> GetGroupAsync(
            [FromRoute] string key,
            [FromServices] ICoreUnitOfWork uow,
            [FromServices] IWorkContext workContext)
        {
            var tenantCode = await workContext.GetTenantCodeAsync();

            var settingRepo = uow.GetRepository<ISettingRepository>();
            var settings = await settingRepo.GetByGroupAsync(tenantCode, key);
            return Ok(settings.Select(x => new
            {
                x.Id,
                x.Key,
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
            var tenantCode = await workContext.GetTenantCodeAsync();

            var settingRepo = uow.GetRepository<ISettingRepository>();
            var setting = await settingRepo.GetByKeyAsync(tenantCode, key);
            return Ok(new
            {
                setting.Id,
                setting.Key,
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
            [FromBody] List<SettingModel> command,
            [FromServices] ICoreUnitOfWork uow,
            [FromServices] IWorkContext workContext,
            [FromServices] ISettingService settingService,
            [FromServices] IEventFactory eventFactory)
        {
            var tenantCode = await workContext.GetTenantCodeAsync();
            var usercode = workContext.GetUserCode();

            //phân biệt ra sửa và thêm
            //Sửa setting
            var repo = uow.GetRepository<ISettingRepository>();
            var entities = await repo.GetByGroupTenantAsync(tenantCode, group);
            var entityByKey = entities.GroupBy(x => x.Key, x => x)
                                       .ToDictionary(x => x.Key, x => x.FirstOrDefault());

            var defaultSettings = settingService.GetDefaultSettings();

            var inserts = new List<Setting>();
            var updates = new List<Setting>();
            foreach (var item in command)
            {
                if (entityByKey.ContainsKey(item.Key))
                {
                    //sửa
                    var setting = entityByKey[item.Key];

                    setting.Value = item.Value;
                    setting.UpdatedAt = DateTime.Now;
                    setting.UpdatedAtUtc = DateTime.UtcNow;
                    setting.UpdatedBy = usercode;

                    updates.Add(setting);
                }
                else
                {
                    //thêm
                    //Setting ko có trong default => bỏ qua
                    var defaultSetting = defaultSettings.FirstOrDefault(x => x.Key == item.Key);
                    if (defaultSetting == null)
                        continue;

                    var setting = new Setting
                    {
                        Code = Guid.NewGuid(),
                        Group = group,
                        Key = item.Key,
                        Name = defaultSetting.Name,
                        Value = item.Value,
                        Options = defaultSetting.Options,
                        Type = defaultSetting.Type,
                        CreatedAt = DateTime.Now,
                        CreatedAtUtc = DateTime.UtcNow,
                        CreatedBy = usercode,
                        Description = defaultSetting.Description,
                        TenantCode = tenantCode,
                    };
                    inserts.Add(setting);
                }
            }

            var hasChange = false;
            if (inserts.Count > 0)
            {
                await repo.InsertsAsync(inserts);
                hasChange = true;
            }

            if (updates.Count > 0)
            {
                repo.Updates(updates);
                hasChange = true;
            }

            if (!hasChange)
                return Ok();

            await uow.CommitAsync();

            //Notification
            eventFactory.GetOrAddEvent<IEvent<SettingUpdatedEventModel>, ISettingUpdatedEvent>().ForEach(async (e) =>
            {
                await e.PublishAsync(new SettingUpdatedEventModel
                {
                    Group = group
                });
            });
            return Ok();
        }
    }
}
