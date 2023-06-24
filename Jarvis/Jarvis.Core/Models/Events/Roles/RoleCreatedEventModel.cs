using System;

namespace Jarvis.Core.Models.Events.Roles
{
    public class RoleCreatedEventModel
    {
        public Guid TenantKey { get; set; }
        public Guid RoleKey { get; set; }
        public string Name { get; set; }
    }
}