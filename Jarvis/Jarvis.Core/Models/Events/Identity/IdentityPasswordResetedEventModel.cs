using System;

namespace Jarvis.Core.Models.Events.Identity
{
    public class IdentityPasswordResetedEventModel
    {
        public Guid IdUser { get; set; }
        public string Password { get; set; }
    }
}