using System;

namespace Jarvis.Core.Models.Events.Users
{
    public class UserUnlockedEventModel
    {
        public Guid TenantCode { get; set; }
        public Guid IdUser { get; set; }
    }
}