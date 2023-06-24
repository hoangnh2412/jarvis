using System;
using Jarvis.Core.Database.Poco;

namespace Jarvis.Core.Models.Emails
{
    public class EmailHistoryPagingOutput
    {
        public Guid Key { get; set; }
        public Guid TenantCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }

        public string Code { get; set; }
        public string Cc { get; set; }
        public string Bcc { get; set; }
        public string Metadata { get; set; }
        public int Type { get; set; }

        public string From { get; set; }
        public string To { get; set; }
        public int Status { get; set; }

        public static EmailHistoryPagingOutput MapToModel(EmailHistory entity)
        {
            if (entity == null)
                return null;

            return new EmailHistoryPagingOutput
            {
                Key = entity.Key,
                TenantCode = entity.TenantCode,
                CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy,
                Code = entity.Code,
                Cc = entity.Cc,
                Bcc = entity.Bcc,
                Metadata = entity.Metadata,
                Type = entity.Type,
                From = entity.From,
                To = entity.To,
                Status = entity.Status
            };
        }
    }
}