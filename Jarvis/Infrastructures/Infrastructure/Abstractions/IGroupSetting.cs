using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Abstractions
{
    public interface IGroupSetting
    {
        List<string> GetValues();

        Dictionary<string, string> GetDictionary();
    }
}
