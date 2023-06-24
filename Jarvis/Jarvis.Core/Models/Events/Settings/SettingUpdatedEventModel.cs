using System;

namespace Jarvis.Core.Models.Events.Settings
{
    public class SettingUpdatedEventModel
    {
        public Guid TenantKey { get; set; }
        public Guid UserKey { get; set; }
        public string Group { get; set; }
    }
}