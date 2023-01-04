using System;

namespace Jarvis.Core.Models.Events.Users
{
    public class UserLockedEventModel
    {
        public Guid TenantCode { get; set; }
        public Guid IdUser { get; set; }
    }
}