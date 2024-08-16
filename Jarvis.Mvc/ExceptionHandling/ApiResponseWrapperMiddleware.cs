using System.Net;
using System.Text;
using Jarvis.Domain.Shared.Enums;
using Jarvis.Domain.Shared.ExceptionHandling;
using Jarvis.Domain.Shared.RequestResponse;
using Jarvis.Mvc.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Jarvis.Mvc.ExceptionHandling;

public class ApiResponseWrapperMiddleware(
    ILogger<ApiResponseWrapperMiddleware> logger,
    RequestDelegate next)
{
    private readonly ILogger<ApiResponseWrapperMiddleware> _logger = logger;
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var requestBody = await context.Request.GetRequestBodyAsync();
        var originalResponseBodyStream = context.Response.Body;

        using (var stream = new MemoryStream())
        {
            try
            {
                context.Response.Body = stream;
                await _next(context);

                if (context.Response.HasStarted)
                    return;

                var responseBody = await HttpContextExtension.ReadResponseBodyStreamAsync(stream);
                context.Response.Body = originalResponseBodyStream;
                context.Response.ContentType = "application/json";

                var responseContent = GenerateResponseContent(context, responseBody);
                context.Response.ContentLength = Encoding.UTF8.GetByteCount(responseContent);
                await context.Response.WriteAsync(responseContent);
            }
            catch (Exception ex)
            {
                if (context.Response.HasStarted)
                    return;

                _logger.LogError(ex, "{message}", ex.Message);

                var responseContent = GenerateResponseContent(context, ex);
                context.Response.StatusCode = responseContent.Item1.GetHashCode();
                context.Response.ContentLength = Encoding.UTF8.GetByteCount(responseContent.Item2);
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(responseContent.Item2);

                stream.Seek(0, SeekOrigin.Begin);
                await stream.CopyToAsync(originalResponseBodyStream);
            }
        }
    }

    private static (HttpStatusCode, string) GenerateResponseContent(HttpContext context, Exception ex)
    {
        var code = BaseErrorCode.Default500;
        var statusCode = HttpStatusCode.InternalServerError;

        if (ex is BusinessException)
            (statusCode, code) = ParseStatusCode<BusinessException>(ex);

        if (ex is ConflictException)
            (statusCode, code) = ParseStatusCode<ConflictException>(ex);

        if (ex is NotFoundException)
            (statusCode, code) = ParseStatusCode<NotFoundException>(ex);

        return (statusCode, JsonConvert.SerializeObject(new BaseResponse(context.TraceIdentifier, code)));
    }

    private static (HttpStatusCode, string) ParseStatusCode<T>(Exception ex) where T : BusinessException
    {
        if (ex is not T)
            return (HttpStatusCode.InternalServerError, BaseErrorCode.Default500);

        if (ex is T businessException)
            return (businessException.HttpStatusCode, businessException.Code);

        return (HttpStatusCode.InternalServerError, BaseErrorCode.Default500);
    }

    private string GenerateResponseContent(HttpContext context, string content)
    {
        if (context.Response.StatusCode == HttpStatusCode.OK.GetHashCode())
            return ParseSuccessContent(context, content, BaseErrorCode.Default200);

        if (context.Response.StatusCode == HttpStatusCode.BadRequest.GetHashCode())
            return ParseBadRequest(context, content);

        if (context.Response.StatusCode == HttpStatusCode.Unauthorized.GetHashCode())
            return ParseErrorContent(context, content, BaseErrorCode.Default401);

        if (context.Response.StatusCode == HttpStatusCode.Forbidden.GetHashCode())
            return ParseErrorContent(context, content, BaseErrorCode.Default403);

        if (context.Response.StatusCode == HttpStatusCode.NotFound.GetHashCode())
            return ParseErrorContent(context, content, BaseErrorCode.Default404);

        return content;
    }

    private static string ParseSuccessContent(HttpContext context, string content, string defaultCode)
    {
        var response = new BaseResponse<object>(context.TraceIdentifier, defaultCode);

        if (!string.IsNullOrEmpty(content))
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            response.Data = JsonConvert.DeserializeObject<object>(content);
#pragma warning restore CS8601 // Possible null reference assignment.
        }

        return JsonConvert.SerializeObject(response);
    }

    private static string ParseErrorContent(HttpContext context, string content, string defaultCode)
    {
        if (string.IsNullOrEmpty(content))
            content = defaultCode;

        return JsonConvert.SerializeObject(new BaseResponse(context.TraceIdentifier, content));
    }

    private string ParseBadRequest(HttpContext context, string content)
    {
        var response = new BaseResponse(context.TraceIdentifier, BaseErrorCode.Default400);

        try
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            response = JsonConvert.DeserializeObject<BaseResponse>(content);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            if (response == null)
                _logger.LogError("Cannot deserialize: {content}", content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{message}", ex.Message);
        }

        return JsonConvert.SerializeObject(response);
    }
}