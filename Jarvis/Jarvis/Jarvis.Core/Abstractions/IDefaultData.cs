using System.Collections.Generic;
using Jarvis.Core.Database.Poco;

namespace Jarvis.Core.Abstractions
{
    public interface IDefaultData
    {
        List<Setting> GetSettings();
    }
}
