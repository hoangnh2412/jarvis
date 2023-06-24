using Jarvis.Core.Database.Poco;

namespace Jarvis.Core.Models.Emails
{
    public class EmailTemplateCreateInput
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public string To { get; set; }
        public string Cc { get; set; }
        public string Bcc { get; set; }
        public string Attachments { get; set; }
        public string Metadata { get; set; }
        public int Type { get; set; }

        public static EmailTemplate MapToEntity(EmailTemplateCreateInput model)
        {
            if (model == null)
                return null;

            return new EmailTemplate
            {
                Code = model.Code,
                Name = model.Name,
                Subject = model.Subject,
                Content = model.Content,
                To = model.To,
                Cc = model.Cc,
                Bcc = model.Bcc,
                Attachments = model.Attachments,
                Metadata = model.Metadata,
                Type = model.Type
            };
        }
    }
}