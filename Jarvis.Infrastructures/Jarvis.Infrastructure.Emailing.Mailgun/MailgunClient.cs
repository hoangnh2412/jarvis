using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Options;

namespace Jarvis.Infrastructure.Emailing.Mailgun;

public class MailgunClient : IMailgunClient
{
    private readonly HttpClient _client;
    private readonly HttpOption _options;

    public MailgunClient(
        HttpClient client,
        IOptions<HttpOption> options)
    {
        _client = client;
        _options = options.Value;
    }

    public async Task SendAsync<T>(string subject, string content, string[] to, string[] cc, string[] bcc, Attachment[] attachments, T option = default)
    {
        var formContent = new MultipartFormDataContent
        {
            { new StringContent($"{_options.FromName}<{_options.From}>"), "from" },
            { new StringContent(subject), "subject" },
            { new StringContent(content), "html" },
            { new StringContent("true"), "o:tracking" },
            { new StringContent("true"), "o:tracking-opens" },
            { new StringContent("true"), "o:tracking-clicks" }
        };

        if (to.Length > 0)
        {
            foreach (var item in to)
            {
                formContent.Add(new StringContent(item), "to");
            }
        }

        if (cc != null && cc.Length > 0)
        {
            foreach (var item in cc)
            {
                formContent.Add(new StringContent(item), "cc");
            }
        }

        if (bcc != null && bcc.Length > 0)
        {
            foreach (var item in bcc)
            {
                formContent.Add(new StringContent(item), "bcc");
            }
        }

        if (attachments.Length > 0)
        {
            foreach (var item in attachments)
            {
                var memoryStream = item.ContentStream as MemoryStream;
                var fileContent = new ByteArrayContent(memoryStream.ToArray());

                fileContent.Headers.ContentType = new MediaTypeHeaderValue(item.ContentType.MediaType);
                fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "attachment",
                    FileName = item.Name,

                };
                formContent.Add(fileContent);
            }
        }

        var request = new HttpRequestMessage(HttpMethod.Post, new Uri($"{_options.Host}/{_options.UserName}/messages"));
        request.Content = formContent;
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"api:{_options.Password}")));

        var response = await _client.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Send mail with mailgun error with " +
                $"  StatusCode: {response.StatusCode} - ReasonPhrase: {response.ReasonPhrase} - Content: {responseContent}");
        }
    }

    public async Task SendAsync<T>(string subject, string content, string to, string[] cc, string[] bcc, Attachment[] attachments, T option = default)
    {
        await SendAsync<T>(subject, content, new string[1] { to }, cc, bcc, attachments, option);
    }
}