using Infrastructure.Database.Abstractions;
using Infrastructure.Database.Models;
using System;

namespace Jarvis.Core.Database.Poco
{
    public class OrganizationUser : IEntity<int>
    {
        public int Id { get; set; }
        public Guid Key { get; set; }
        public Guid IdUser { get; set; }
        public int Level { get; set; }
        public Guid OrganizationCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public Guid CreatedBy { get; set; }
    }
}
