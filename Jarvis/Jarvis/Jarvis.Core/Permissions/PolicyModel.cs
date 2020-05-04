using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Jarvis.Core.Constants;

namespace Jarvis.Core.Permissions
{
    public class PolicyModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string GroupCode { get; set; }
        public string GroupName { get; set; }
        public int GroupOrder { get; set; }
        public string ModuleCode { get; set; }
        public string ModuleName { get; set; }
        public int ModuleOrder { get; set; }
        public List<ClaimOfResource> ClaimOfResource { get; set; }
        public List<ClaimOfChildResource> ClaimOfChildResources { get; set; }
    }
}
