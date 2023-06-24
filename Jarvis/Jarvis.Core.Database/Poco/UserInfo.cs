using Infrastructure.Database.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jarvis.Core.Database.Poco
{
    public class UserInfo : IEntity<int>
    {
        public int Id { get; set; }
        public Guid Key { get; set; }
        public string FullName { get; set; }
        public string AvatarPath { get; set; }
    }
}
