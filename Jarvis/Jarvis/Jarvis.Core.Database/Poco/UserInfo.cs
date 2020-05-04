using Infrastructure.Database.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jarvis.Core.Database.Poco
{
    public class UserInfo : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string AvatarPath { get; set; }
    }
}
