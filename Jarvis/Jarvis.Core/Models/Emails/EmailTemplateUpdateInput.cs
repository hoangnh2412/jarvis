using Jarvis.Core.Database.Poco;

namespace Jarvis.Core.Models.Emails
{
    public class EmailTemplateUpdateInput
    {
        public string Name { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public string To { get; set; }
        public string Cc { get; set; }
        public string Bcc { get; set; }
        public string Attachments { get; set; }
        public string Metadata { get; set; }
        public int Type { get; set; }

        public static EmailTemplate MapToEntity(EmailTemplateUpdateInput model, EmailTemplate entity)
        {
            entity.Name = model.Name;
            entity.Subject = model.Subject;
            entity.Content = model.Content;
            entity.To = model.To;
            entity.Cc = model.Cc;
            entity.Bcc = model.Bcc;
            entity.Attachments = model.Attachments;
            entity.Metadata = model.Metadata;
            entity.Type = model.Type;
            return entity;
        }
    }
}