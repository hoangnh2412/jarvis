using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Extensions
{
    public static class HttpContextExtension
    {
        public static async Task<string> ReadBodyRequestAsync(this HttpContext httpContext)
        {
            string body = null;

            var req = httpContext.Request;

            // Allows using several time the stream in ASP.Net Core
            req.EnableBuffering();

            // Arguments: Stream, Encoding, detect encoding, buffer size 
            // AND, the most important: keep stream opened
            using (System.IO.StreamReader reader = new System.IO.StreamReader(req.Body, Encoding.UTF8, true, 1024, true))
            {
                body = await reader.ReadToEndAsync();
            }

            // Rewind, so the core is not lost when it looks at the body for the request
            req.Body.Position = 0;

            return body;
        }
    }
}