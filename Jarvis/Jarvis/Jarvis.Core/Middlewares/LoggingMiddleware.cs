using Jarvis.Core.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Jarvis.Core.Middlewares
{
    public class LoggingMiddleware
    {
        private readonly ILogger<LoggingMiddleware> _logger;
        private readonly RequestDelegate _next;
        private readonly List<string> _fileContentTypes = new List<string> { ContentType.Pdf, ContentType.Xml, ContentType.Zip, ContentType.Stream };

        public LoggingMiddleware(
            ILogger<LoggingMiddleware> logger,
            RequestDelegate next)
        {
            _logger = logger;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            Stream originalRequestBody = context.Request.Body;
            Stream originalResponseBody = context.Response.Body;
            string requestBody = null;
            string responseBody = null;

            try
            {
                //Nếu là file sẽ ko đọc request body
                if (context.Request.ContentType == null || !context.Request.ContentType.Contains(ContentType.Files))
                {
                    requestBody = await GetRequestBodyAsync(context.Request);

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
                _logger.LogError(ex, ex.Message);

                if (ex.InnerException != null)
                    _logger.LogError(ex.InnerException, ex.InnerException.Message);

                throw ex;
            }
            finally
            {
                context.Request.Body = originalRequestBody;
                context.Response.Body = originalResponseBody;
            }

            watch.Stop();
            _logger.LogCritical($"| {watch.ElapsedMilliseconds}");
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
