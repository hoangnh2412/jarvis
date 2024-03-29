﻿using Infrastructure.Database.EntityFramework;
using Jarvis.Core.Database.Poco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Jarvis.Core.Permissions;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using Newtonsoft.Json;
using Infrastructure.Caching;
using Microsoft.Extensions.Options;

namespace Jarvis.Core.Database.Repositories.EntityFramework
{
    public class SettingRepository : EntityRepository<Setting>, ISettingRepository
    {
        private readonly ICacheService _cache;

        public SettingRepository(
            ICacheService cache)
        {
            _cache = cache;
        }

        public async Task<List<Setting>> GetByGroupAsync(Guid tenantCode, string group)
        {
            IQueryable<Setting> query = Query.Where(x => x.Group == group);
            query = query.Where(x => x.TenantCode == Guid.Empty || x.TenantCode == tenantCode);

            var settings = await query.AsQueryable().ToListAsync();
            return RemoveDefaultSettings(tenantCode, settings);
        }

        public async Task<List<Setting>> GetByGroupTenantAsync(Guid tenantCode, string group)
        {
            IQueryable<Setting> query = Query.Where(x => x.Group == group);
            query = query.QueryByTenantCode(tenantCode);
            return await query.AsQueryable().ToListAsync();
        }


        public async Task<List<Setting>> GetByTenantCodeAsync(Guid tenantCode)
        {
            IQueryable<Setting> query = Query.Where(x => x.TenantCode == Guid.Empty || x.TenantCode == tenantCode);

            var settings = await query.AsQueryable().ToListAsync();
            return RemoveDefaultSettings(tenantCode, settings);
        }

        public async Task<List<Setting>> GetByTenantCodeAsync(string cacheKey, Guid tenantCode)
        {
            if (cacheKey == null)
                return await GetByTenantCodeAsync(tenantCode);

            var settings = await _cache.HashGetAsync<Setting>(cacheKey);
            if (settings.Count != 0)
                return settings;

            settings = await GetByTenantCodeAsync(tenantCode);
            if (settings.Count != 0)
                await _cache.HashSetAsync(cacheKey, settings.ToDictionary(x => x.Code.ToString(), x => JsonConvert.SerializeObject(x)));

            return settings;
        }

        public async Task<Setting> GetByKeyAsync(Guid tenantCode, string code)
        {
            IQueryable<Setting> query = Query.Where(x => x.Code == code);
            query = query.Where(x => x.TenantCode == Guid.Empty || x.TenantCode == tenantCode);

            var settings = await query.AsQueryable().ToListAsync();
            settings = RemoveDefaultSettings(tenantCode, settings);

            return settings.FirstOrDefault();
        }

        public async Task<Setting> GetByKeyAsync(string cacheKey, Guid tenantCode, string key)
        {
            if (cacheKey == null)
                return await GetByKeyAsync(tenantCode, key);

            Setting setting;
            var bytes = await _cache.GetAsync(cacheKey);
            if (bytes == null)
            {
                setting = await GetByKeyAsync(tenantCode, key);
                await _cache.SetAsync(cacheKey, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(setting)));
                return setting;
            }

            var data = Encoding.UTF8.GetString(bytes);
            setting = JsonConvert.DeserializeObject<Setting>(data);
            return setting;
        }

        public async Task<Setting> GetByKeyAsync(string code)
        {
            IQueryable<Setting> query = Query.Where(x => x.Code == code);
            query = query.QueryByTenantCode(Guid.Empty);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<Setting>> GetByTenantCodesAsync(List<Guid> tenantCodes)
        {
            return await Query.Query(x => tenantCodes.Contains(x.TenantCode) && !x.IsReadOnly).ToListAsync();
        }

        private static List<Setting> RemoveDefaultSettings(Guid tenantCode, List<Setting> entities)
        {
            var settings = new List<Setting>();
            var groups = entities.GroupBy(x => x.Code);
            foreach (var group in groups)
            {
                //Nếu tất cả là setting mặc định => dùng setting default
                if (group.All(x => x.TenantCode == Guid.Empty))
                {
                    settings.AddRange(group.ToList());
                }
                else //Nếu đã có setting theo tenant => dùng setting tenant
                {
                    settings.AddRange(group.Where(x => x.TenantCode == tenantCode).ToList());
                }
            }

            return settings;
        }
    }
}
