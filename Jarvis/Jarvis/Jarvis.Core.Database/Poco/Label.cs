using Infrastructure.Database.Abstractions;
using Infrastructure.Database.Models;
using System;

namespace Jarvis.Core.Database.Poco
{
    public class Label : IEntity<int>, ITenantEntity, ILogCreatedEntity, ILogUpdatedEntity, ILogDeletedEntity, ILogDeletedVersionEntity<int?>, IPermissionEntity
    {
        public int Id { get; set; }
        public Guid TenantCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? DeletedAtUtc { get; set; }
        public Guid? DeletedBy { get; set; }
        public int? DeletedVersion { get; set; }


        public Guid Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
    }
}
