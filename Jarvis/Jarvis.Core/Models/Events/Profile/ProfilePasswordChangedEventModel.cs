using System;

namespace Jarvis.Core.Models.Events.Profile
{
    public class ProfilePasswordChangedEventModel
    {
        public Guid UserKey { get; set; }
        public string Password { get; set; }
    }
}