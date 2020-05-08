using Infrastructure.Database.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jarvis.Core.Database.Poco
{
    public class Entity : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public Guid IdTenant { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        public string Slug { get; set; }
        public string Description { get; set; }
        public string EntityId { get; set; }

        public Guid IdEntityType { get; set; }
        public virtual EntityType EntityType { get; set; }
    }
}
