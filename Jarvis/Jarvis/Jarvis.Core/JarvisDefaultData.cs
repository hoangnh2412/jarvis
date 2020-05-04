using System.Collections.Generic;
using Jarvis.Core.Abstractions;
using Jarvis.Core.Database.Poco;

namespace Jarvis.Core
{
    public class JarvisDefaultData : IDefaultData
    {
        public List<Setting> GetSettings()
        {
            var settings = new List<Setting>();

            settings.AddRange(TenantSettings);

            return settings;
        }

        private List<Setting> TenantSettings = new List<Setting>
        {

        };
    }
}
