using Infrastructure.Database.Abstractions;
using System;
using System.Collections.Generic;

namespace Jarvis.Core.Database.Poco
{
    public class EntityType : IEntity<int>
    {
        public int Id { get; set; }
        public Guid Key { get; set; }
        public Guid IdTenant { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        public string RoutingController { get; set; }
        public string RoutingAction { get; set; }

        public virtual ICollection<Entity> Entities { get; set; }
    }
}
