using System;
using System.Collections.Generic;
using Jarvis.Core.Database.Poco;
using Newtonsoft.Json;

namespace Jarvis.Core.Models.Emails
{
    public class EmailTemplateModel
    {
        public Guid Key { get; set; }
        public Guid TenantCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public Guid CreatedBy { get; set; }

        public string Code { get; set; }
        public string Name { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public string To { get; set; }
        public string[] Cc { get; set; }
        public string[] Bcc { get; set; }
        public Dictionary<string, byte[]> Attachments { get; set; }
        public string Metadata { get; set; }
        public int Type { get; set; }

        public static EmailTemplateModel MapToModel(EmailTemplate entity)
        {
            if (entity == null)
                return null;

            return new EmailTemplateModel
            {
                Key = entity.Key,
                TenantCode = entity.TenantCode,
                CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy,
                Code = entity.Code,
                Name = entity.Name,
                Subject = entity.Subject,
                Content = entity.Content,
                To = entity.To,
                Cc = string.IsNullOrEmpty(entity.Cc) ? null : JsonConvert.DeserializeObject<string[]>(entity.Cc),
                Bcc = string.IsNullOrEmpty(entity.Bcc) ? null : JsonConvert.DeserializeObject<string[]>(entity.Bcc),
                Attachments = string.IsNullOrEmpty(entity.Attachments) ? null : JsonConvert.DeserializeObject<Dictionary<string, byte[]>>(entity.Attachments),
                Metadata = entity.Metadata,
                Type = entity.Type
            };
        }
    }
}