using Infrastructure.Database.Abstractions;
using Infrastructure.Database.Models;
using System;

namespace Jarvis.Core.Database.Poco
{
    public class OrganizationRole : IEntity<int>
    {
        public int Id { get; set; }
        public Guid Key { get; set; }
        public Guid IdRole { get; set; }
        public Guid OrganizationCode { get; set; }
    }
}
