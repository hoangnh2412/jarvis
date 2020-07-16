using System;

namespace Jarvis.Core.Models.Events.Identity
{
    public class IdentityPasswordForgotedEventModel
    {
        public Guid TenantCode { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public Guid IdUser { get; set; }
        public string HostName { get; set; }
    }
}