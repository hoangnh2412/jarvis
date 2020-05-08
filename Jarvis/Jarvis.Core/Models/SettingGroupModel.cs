using Jarvis.Core.Database.Poco;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jarvis.Core.Models
{
    public class SettingGroupModel
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public List<SettingModel> Settings { get; set; }
    }
}
