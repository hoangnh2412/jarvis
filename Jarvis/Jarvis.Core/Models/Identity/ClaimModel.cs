using System.Collections.Generic;

namespace Jarvis.Models.Identity.Models.Identity
{
    public class ClaimModel
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public string GroupCode { get; set; }
        public string GroupName { get; set; }

        public string ModuleCode { get; set; }
        public string ModuleName { get; set; }

        public bool Selected { get; set; }

        // public string Resource { get; set; }
        // public Dictionary<string, string> Resources { get; set; }
        
        // public string ChildResource { get; set; }
        // public Dictionary<string, string> ChildResources { get; set; }
    }
}
