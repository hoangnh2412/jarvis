using System;

namespace Jarvis.Core.Models.Events.Roles
{
    public class RoleCreatedEventModel
    {
        public Guid TenantCode { get; set; }
        public string Name { get; set; }
    }
}