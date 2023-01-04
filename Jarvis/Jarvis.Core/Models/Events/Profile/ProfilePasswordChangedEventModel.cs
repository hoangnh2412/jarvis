using System;

namespace Jarvis.Core.Models.Events.Profile
{
    public class ProfilePasswordChangedEventModel
    {
        public Guid IdUser { get; set; }
        public string Password { get; set; }
    }
}