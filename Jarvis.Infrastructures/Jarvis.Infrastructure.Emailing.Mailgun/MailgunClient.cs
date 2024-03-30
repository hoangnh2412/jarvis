using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Jarvis.Infrastructure.Emailing.Mailgun;

public class MailgunClient
{
    private readonly HttpClient _client;
    private readonly SmtpOption _options;

    public MailgunClient(
        HttpClient client,
        IOptions<SmtpOption> options)
    {
        _client = client;
        _options = options.Value;

        _client.BaseAddress = new Uri(options.Value.Host);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"api:{options.Value.Password}")));
    }

    public void Send()
    {
        var formContent = new MultipartFormDataContent
            {
                { new StringContent($"{_options.FromName}<{_options.From}>"), "from" },
                // { new StringContent(message.ToAdress.Address), "to" },
                // { new StringContent(message.Subject), "subject" },
                // { new StringContent(message.Body), "html" },
                { new StringContent("true"), "o:tracking" },
                { new StringContent("true"), "o:tracking-opens" },
                { new StringContent("true"), "o:tracking-clicks" }
            };

        // if (message.Bcc.Count > 0)
        // {
        //     foreach (var bcc in message.Bcc)
        //     {
        //         formContent.Add(new StringContent(bcc.Address), "bcc");
        //     }
        // }

        // if (message.Attachments.Count > 0)
        // {
        //     foreach (var attach in message.Attachments)
        //     {
        //         var memoryStream = attach.ContentStream as MemoryStream;
        //         var fileContent = new ByteArrayContent(memoryStream.ToArray());

        //         fileContent.Headers.ContentType = new MediaTypeHeaderValue(attach.ContentType.MediaType);
        //         fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        //         {
        //             Name = "attachment",
        //             FileName = attach.Name,

        //         };
        //         formContent.Add(fileContent);
        //     }
        // }

        // var response = await client.PostAsync(url, formContent);
        // if (!response.IsSuccessStatusCode)
        // {
        //     throw new Exception($"Send mail with mailgun error with " +
        //         $"  StatusCode:{response.StatusCode} - ReasonPhrase:{response.ReasonPhrase}");
        // }
    }
}