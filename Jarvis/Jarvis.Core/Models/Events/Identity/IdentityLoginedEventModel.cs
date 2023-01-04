using System;

namespace Jarvis.Core.Models.Events.Identity
{
    public class IdentityLoginedEventModel
    {
        public Guid TenantCode { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}