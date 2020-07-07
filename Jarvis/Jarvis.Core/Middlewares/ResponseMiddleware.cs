using Jarvis.Core.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Jarvis.Core.Middlewares
{
    public class ResponseMiddleware
    {
        private readonly ILogger<ResponseMiddleware> _logger;
        private readonly RequestDelegate _next;
        private readonly List<string> _fileContentTypes = new List<string> { ContentType.Pdf, ContentType.Xml, ContentType.Zip, ContentType.Stream };

        public ResponseMiddleware(
            ILogger<ResponseMiddleware> logger,
            RequestDelegate next)
        {
            _logger = logger;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            Stream originalResponseBody = context.Response.Body;
            string responseBody = null;

            try
            {
                //Nếu là file sẽ ko đọc request body
                if (context.Request.ContentType == null || !context.Request.ContentType.Contains(ContentType.Files))
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        context.Response.Body = stream;
                        await _next(context);
                        responseBody = await GetResponseBodyAsync(context.Response);
                        await stream.CopyToAsync(originalResponseBody);
                    }
                }
                else
                {
                    await _next(context);
                }
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "text/plain;charset=UTF-8";
                
                var bytes = Encoding.UTF8.GetBytes(ex.Message);
                using (var output = new MemoryStream(bytes))
                {
                    await output.CopyToAsync(originalResponseBody);
                }
            }
            finally
            {
                context.Response.Body = originalResponseBody;
            }
        }

        private static async Task<string> GetRequestBodyAsync(HttpRequest request)
        {
            request.EnableBuffering();
            request.Body.Seek(0, SeekOrigin.Begin);
            var text = await new StreamReader(request.Body).ReadToEndAsync();
            request.Body.Seek(0, SeekOrigin.Begin);
            return text;
        }

        private static async Task<string> GetResponseBodyAsync(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var text = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);

            return text;
        }
    }
}
