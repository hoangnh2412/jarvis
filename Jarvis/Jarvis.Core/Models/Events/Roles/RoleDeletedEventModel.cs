using System;

namespace Jarvis.Core.Models.Events.Roles
{
    public class RoleDeletedEventModel
    {
        public Guid TenantCode { get; set; }
        public Guid IdRole { get; set; }
    }
}