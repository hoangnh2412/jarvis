using System.Text;
using Microsoft.AspNetCore.Http;

namespace Jarvis.Shared.Extensions;

/// <summary>
/// Provides extension functions for HttpContext
/// </summary>
public static partial class HttpContextExtension
{
    /// <summary>
    /// Return body of request
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
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

    public static async Task<string> GetRequestBodyAsync(this HttpRequest request)
    {
        var httpMethodsWithRequestBody = new[] { "POST", "PUT", "PATCH" };
        var hasRequestBody = httpMethodsWithRequestBody.Any(x => x.Equals(request.Method.ToUpper()));
        string requestBody = default;

        if (hasRequestBody)
        {
            request.EnableBuffering();

            using var memoryStream = new MemoryStream();
            await request.Body.CopyToAsync(memoryStream);
            requestBody = Encoding.UTF8.GetString(memoryStream.ToArray());
            request.Body.Seek(0, SeekOrigin.Begin);
        }
        return requestBody;
    }

    public static async Task<string> ReadResponseBodyStreamAsync(Stream bodyStream)
    {
        bodyStream.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(bodyStream).ReadToEndAsync();
        bodyStream.Seek(0, SeekOrigin.Begin);

        var (IsEncoded, ParsedText) = responseBody.VerifyBodyContent();

        return IsEncoded ? ParsedText : responseBody;
    }

    public static bool IsSwagger(this HttpRequest request, string swaggerPath)
    {
        return request.Path.StartsWithSegments(new PathString(swaggerPath));
    }

    public static bool IsExclude(this HttpRequest request, params string[] excludePaths)
    {
        if (excludePaths == null || excludePaths.Count() == 0)
            return false;

        return excludePaths.Any(x => x == request.Path.Value);
    }

    public static bool AllowContentType(this string contentType, params string[] contentTypes)
    {
        if (contentTypes == null || contentTypes.Count() == 0)
            return false;

        if (contentType == null)
            return true;

        return contentTypes.Any(x => x == contentType || contentType.Contains(x));
    }
}