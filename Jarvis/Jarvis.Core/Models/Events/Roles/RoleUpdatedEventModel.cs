using System;

namespace Jarvis.Core.Models.Events.Roles
{
    public class RoleUpdatedEventModel
    {
        public Guid TenantCode { get; set; }
        public Guid IdRole { get; set; }
        public string Name { get; set; }
    }
}