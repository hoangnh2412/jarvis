using Infrastructure.Database.Abstractions;
using Microsoft.AspNetCore.Identity;
using System;

namespace Infrastructure.Database.Entities
{
    public class User : IdentityUser<Guid>, IEntity<Guid>, ITenantEntity, ILogCreatedEntity, ILogUpdatedEntity, ILogDeletedEntity, ILogDeletedVersionEntity<Guid?>, IPermissionEntity
    {
        public Guid Key { get; set; }
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
        public Guid? DeletedVersion { get; set; }

        /// <summary>
        /// Loại người dùng: SuperAdmin, Admin, User
        /// </summary>
        /// <value></value>
        public int Type { get; set; }
    }
}
