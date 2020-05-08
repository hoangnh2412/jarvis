using System;
using System.Collections.Generic;

namespace Jarvis.Core.Models
{
    public class HierarchyTenantModel
    {
        public int Id { get; set; }
        public Guid Code { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string HierarchyName { get; set; }
        public string[] HierarchyNames { get; set; }
        public string Path { get; set; }
        public List<string> Codes { get; set; }
        public int Level { get; set; }
        public Guid? IdParent { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
