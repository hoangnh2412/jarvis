using System;

namespace Jarvis.Core.Models.Events.Identity
{
    public class IdentityPasswordResetedEventModel
    {
        public Guid TenantKey { get; set; }
        public Guid UserKey { get; set; }
        public string Password { get; set; }
    }
}