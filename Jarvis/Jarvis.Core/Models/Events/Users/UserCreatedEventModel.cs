using System;

namespace Jarvis.Core.Models.Events.Users
{
    public class UserCreatedEventModel
    {
        public Guid TenantCode { get; set; }
        public Guid IdUser { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool IsRandomPassword { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
    }
}