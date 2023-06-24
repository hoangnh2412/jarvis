using System;
using Jarvis.Core.Database.Poco;

namespace Jarvis.Core.Models.Emails
{
    public class EmailTemplatePagingOutput
    {
        public Guid Key { get; set; }
        public Guid TenantCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }

        public string Code { get; set; }
        public string Name { get; set; }
        public string To { get; set; }
        public string Cc { get; set; }
        public string Bcc { get; set; }
        public string Metadata { get; set; }
        public int Type { get; set; }

        public static EmailTemplatePagingOutput MapToModel(EmailTemplate entity)
        {
            if (entity == null)
                return null;

            return new EmailTemplatePagingOutput
            {
                Key = entity.Key,
                TenantCode = entity.TenantCode,
                CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy,
                Code = entity.Code,
                Name = entity.Name,
                To = entity.To,
                Cc = entity.Cc,
                Bcc = entity.Bcc,
                Metadata = entity.Metadata,
                Type = entity.Type
            };
        }
    }
}