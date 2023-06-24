using System;
using Infrastructure.Database.Abstractions;

namespace Jarvis.Core.Database.Poco
{
    public class EmailHistory : IEntity<int>, ITenantEntity, ILogCreatedEntity, ILogUpdatedEntity
    {
        public int Id { get; set; }
        public Guid Key { get; set; }
        public Guid TenantCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public Guid? UpdatedBy { get; set; }

        public string Code { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public string Cc { get; set; }
        public string Bcc { get; set; }
        public string Attachments { get; set; }
        public string Metadata { get; set; }
        public int Type { get; set; }

        public string From { get; set; }
        public string FromName { get; set; }
        public string To { get; set; }
        public int Status { get; set; }
        public Guid ConcurrencyStamp { get; set; }
    }
}