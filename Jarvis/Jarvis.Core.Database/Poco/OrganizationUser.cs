using Infrastructure.Database.Abstractions;
using Infrastructure.Database.Models;
using System;

namespace Jarvis.Core.Database.Poco
{
    public class OrganizationUser : IEntity<int>
    {
        public int Id { get; set; }
        public Guid IdUser { get; set; }
        public bool IsManager { get; set; }
        public Guid OrganizationCode { get; set; }
    }
}
