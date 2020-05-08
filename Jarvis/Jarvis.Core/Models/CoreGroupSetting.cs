using Infrastructure.Abstractions;
using Infrastructure.Extensions;
using Jarvis.Core.Constants;
using System.Collections.Generic;

namespace Jarvis.Core.Models
{
    public class CoreGroupSetting : IGroupSetting
    {
        public List<string> GetValues()
        {
            return EnumExtension.GetAllValues<SettingGroupKey>();
        }

        public Dictionary<string, string> GetDictionary()
        {
            return EnumExtension.ToDictionary<SettingGroupKey>();
        }
    }
}
