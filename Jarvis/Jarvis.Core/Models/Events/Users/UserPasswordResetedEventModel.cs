using System;

namespace Jarvis.Core.Models.Events.Users
{
    public class UserPasswordResetedEventModel
    {
        public Guid TenantCode { get; set; }
        public Guid IdUser { get; set; }

        public string Emails { get; set; }
        public string Password { get; set; }
    }
}