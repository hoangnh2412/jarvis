using System;

namespace Jarvis.Core.Models.Events.Profile
{
    public class ProfileDeletedEventModel
    {
        public Guid UserKey { get; set; }
    }
}