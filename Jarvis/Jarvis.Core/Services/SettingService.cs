using Infrastructure.Abstractions;
using Infrastructure.Caching;
using Infrastructure.Database.Abstractions;
using Jarvis.Core.Abstractions;
using Jarvis.Core.Database;
using Jarvis.Core.Database.Poco;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jarvis.Core.Services
{
    public interface ISettingService
    {
        /// <summary>
        /// Lấy ra tất cả các instance của IGroupSetting
        /// </summary>
        /// <returns></returns>
        Dictionary<string, string> GetGroupSettings();

        /// <summary>
        /// Lấy tất cả các setting default
        /// </summary>
        /// <returns></returns>
        List<Setting> GetDefaultSettings();

        /// <summary>
        /// Lấy setting theo Code
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        Task<Setting> GetByCodeAsync(Guid tenantCode, string code);

        /// <summary>
        /// Lấy nhiều settings theo codes
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="codes"></param>
        /// <returns></returns>
        Task<List<Setting>> GetByCodesAsync(Guid tenantCode, params string[] codes);
    }

    public class SettingService : ISettingService
    {
        private readonly IEnumerable<IGroupSetting> _groupSettings;
        private readonly IEnumerable<IDefaultData> _defaultDatas;
        private readonly ICoreUnitOfWork _uow;
        private readonly ICacheService _cacheService;

        public SettingService(
            IEnumerable<IGroupSetting> groupSettings,
            IEnumerable<IDefaultData> defaultDatas,
            ICoreUnitOfWork uow,
            ICacheService cacheService)
        {
            _groupSettings = groupSettings;
            _defaultDatas = defaultDatas;
            _uow = uow;
            _cacheService = cacheService;
        }

        public Dictionary<string, string> GetGroupSettings()
        {
            var keys = new Dictionary<string, string>();

            foreach (var instance in _groupSettings)
            {
                var items = instance.GetDictionary();
                foreach (var item in items)
                {
                    keys.TryAdd(item.Key, item.Value);
                }
            }

            return keys;
        }

        public List<Setting> GetDefaultSettings()
        {
            var defaultDatas = new List<Setting>();
            foreach (var item in _defaultDatas)
            {
                defaultDatas.AddRange(item.GetSettings());
            }
            return defaultDatas;
        }

        public async Task<Setting> GetByCodeAsync(Guid tenantCode, string code)
        {
            return await _cacheService.QueryHashKeyAsync(
                CacheKey.Build(tenantCode, CacheKey.Setting),
                code,
                async () =>
                {
                    var repo = _uow.GetRepository<IRepository<Setting>>();
                    var settings = await repo.GetQuery().Where(x => x.Code == code && (x.TenantCode == tenantCode || x.TenantCode == Guid.Empty)).ToListAsync();
                    return RemoveDefaultSettings(tenantCode, settings).FirstOrDefault();
                });
        }

        public async Task<List<Setting>> GetByCodesAsync(Guid tenantId, params string[] codes)
        {
            return await _cacheService.QueryHashKeysAsync(
                CacheKey.Build(tenantId, CacheKey.Setting),
                codes.ToList(),
                async () =>
                {
                    var repo = _uow.GetRepository<IRepository<Setting>>();
                    var settings = await repo.GetQuery().Where(x => codes.Contains(x.Code) && (x.TenantCode == tenantId || x.TenantCode == Guid.Empty)).ToListAsync();
                    return RemoveDefaultSettings(tenantId, settings);
                },
                (items) =>
                {
                    return items.ToDictionary(x => x.Code, x => x);
                });
        }

        private static List<Setting> RemoveDefaultSettings(Guid tenantCode, List<Setting> entities)
        {
            var settings = new List<Setting>();

            var groups = entities.GroupBy(x => x.Code);
            foreach (var group in groups)
            {
                //Nếu tất cả là setting mặc định => dùng setting default
                if (group.All(x => x.TenantCode == Guid.Empty))
                    settings.AddRange(group.ToList());
                //Nếu đã có setting theo tenant => dùng setting tenant
                else
                    settings.AddRange(group.Where(x => x.TenantCode == tenantCode).ToList());
            }

            return settings;
        }
    }
}
