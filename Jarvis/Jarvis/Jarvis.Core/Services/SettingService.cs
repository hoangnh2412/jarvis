using Infrastructure;
using Infrastructure.Abstractions;
using Jarvis.Core.Abstractions;
using Jarvis.Core.Database.Poco;
using System.Collections.Generic;

namespace Jarvis.Core.Services
{
    public interface ISettingService
    {

        /// <summary>
        /// lấy ra tất cả các groupsetting
        /// </summary>
        /// <returns></returns>
        Dictionary<string, string> GetGroupSettings();

        /// <summary>
        /// lấy tất cả các setting default
        /// </summary>
        /// <returns></returns>
        List<Setting> GetDefaultSettings();
    }

    public class SettingService : ISettingService
    {
        private readonly IModuleManager _moduleManager;

        public SettingService(IModuleManager moduleManager)
        {
            _moduleManager = moduleManager;
        }

        public Dictionary<string, string> GetGroupSettings()
        {
            var groupSettingKeys = new Dictionary<string, string>();

            var instanceGroupSettingKeys = _moduleManager.GetInstances<IGroupSetting>();
            foreach (var instance in instanceGroupSettingKeys)
            {
                var items = instance.GetDictionary();
                foreach (var item in items)
                {
                    groupSettingKeys.TryAdd(item.Key, item.Value);
                }
            }

            return groupSettingKeys;
        }

        public List<Setting> GetDefaultSettings()
        {
            var instanceDefaultDatas = _moduleManager.GetInstances<IDefaultData>();
            var defaultDatas = new List<Setting>();
            foreach (var item in instanceDefaultDatas)
            {
                defaultDatas.AddRange(item.GetSettings());
            }

            return defaultDatas;
        }
    }
}
