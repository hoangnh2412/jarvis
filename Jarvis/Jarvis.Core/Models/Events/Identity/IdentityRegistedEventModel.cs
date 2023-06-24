using System;

namespace Jarvis.Core.Models.Events.Identity
{
    public class IdentityRegistedEventModel
    {
        public Guid UserKey { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
    }
}