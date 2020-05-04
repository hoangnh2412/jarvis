using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Infrastructure
{
    public class ModuleInfo
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public Assembly Assembly { get; set; }

        public string Path { get; set; }

        public string Version { get; set; }

        public Dictionary<string, string> Configure { get; set; }
    }
}
