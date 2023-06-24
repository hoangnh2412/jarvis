using System;

namespace Jarvis.Core.Models.Events.Identity
{
    public class IdentityPasswordForgotedEventModel
    {
        public Guid TenantKey { get; set; }
        public Guid UserKey { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
    }
}