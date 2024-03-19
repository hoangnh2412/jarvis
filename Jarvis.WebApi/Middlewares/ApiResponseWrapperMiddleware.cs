using System.Diagnostics;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Jarvis.Shared;
using Jarvis.Shared.Constants;
using Jarvis.Shared.Enums;
using Jarvis.Shared.Extensions;

namespace Jarvis.WebApi.Middlewares;
public class ApiResponseWrapperMiddleware
{
    private readonly IStringLocalizer<ApiResponseWrapperMiddleware> _localizer;
    private readonly ILogger<ApiResponseWrapperMiddleware> _logger;
    private readonly RequestDelegate _next;

    private readonly JsonSerializerSettings JsonOptions = new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    };

    public ApiResponseWrapperMiddleware(
        IStringLocalizer<ApiResponseWrapperMiddleware> localizer,
        ILogger<ApiResponseWrapperMiddleware> logger,
        RequestDelegate next)
    {
        _localizer = localizer;
        _logger = logger;
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.IsSwagger("/swagger") || context.Request.IsExclude())
        {
            await _next(context);
            return;
        }

        if (!context.Request.ContentType.AllowContentType(ContentType.Json, ContentType.Xml))
        {
            await _next(context);
            return;
        }

        var stopWatch = Stopwatch.StartNew();
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
                
                var response = GenerateResponse(context, responseBody);
                var json = JsonConvert.SerializeObject(response, JsonOptions);
                
                context.Response.ContentType = ContentType.Json;
                context.Response.ContentLength = Encoding.UTF8.GetByteCount(json);
                await context.Response.WriteAsync(json);
            }
            catch (Exception ex)
            {
                if (context.Response.HasStarted)
                    return;

                _logger.LogError(ex, ex.Message);
                var json = JsonConvert.SerializeObject(CreateResponseException(ex), JsonOptions);
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = ContentType.Json;
                context.Response.ContentLength = Encoding.UTF8.GetByteCount(json);
                await context.Response.WriteAsync(json);

                stream.Seek(0, SeekOrigin.Begin);
                await stream.CopyToAsync(originalResponseBodyStream);

                stopWatch.Stop();
                _logger.LogInformation(
                    $"Source:[{context.Connection.RemoteIpAddress}]"
                    + $" | Request: {context.Request.Method} {context.Request.Scheme} {context.Request.Host}{context.Request.Path} {context.Request.QueryString} "
                    + $" | Status Code: {context.Response.StatusCode}"
                    + $" | Response Time: {stopWatch.ElapsedMilliseconds}ms"
                    + $" | Exception: {ex.Message}"
                    + $" | Body: {requestBody}");
            }
            finally
            {
                stopWatch.Stop();

                _logger.LogInformation(
                    $"Source:[{context.Connection.RemoteIpAddress}]"
                    + $" | Request: {context.Request.Method} {context.Request.Scheme} {context.Request.Host}{context.Request.Path} {context.Request.QueryString} "
                    + $" | Status Code: {context.Response.StatusCode}"
                    + $" | Response Time: {stopWatch.ElapsedMilliseconds}ms"
                    + $" | Body: {requestBody}");
            }
        }
    }

    private BaseResponse<object> GenerateResponse(HttpContext context, string responseBody)
    {
        BaseResponse<object> response;
        if (context.Response.StatusCode == HttpStatusCode.OK.GetHashCode())
        {
            response = CreateResponseSuccess(responseBody);
        }
        else if (context.Response.StatusCode == HttpStatusCode.BadRequest.GetHashCode())
        {
            response = CreateResponseHttpStatus(context.Response.StatusCode, responseBody);
        }
        else if (
            context.Response.StatusCode == HttpStatusCode.Unauthorized.GetHashCode()
            || context.Response.StatusCode == HttpStatusCode.Forbidden.GetHashCode()
            || context.Response.StatusCode == HttpStatusCode.NotFound.GetHashCode())
        {
            response = CreateResponseHttpStatus(context.Response.StatusCode);
        }
        else
        {
            response = CreateResponseError(responseBody);
        }

        return response;
    }

    private BaseResponse<object> CreateResponseError(string responseBody)
    {
        BaseResponse<object> response;
        if (int.TryParse(responseBody, out int code))
        {
            var message = BaseResponseCode.GetValue<BaseResponseCode>(code);
            if (message == null)
                response = BaseResponse<object>.ErrorMessage(code, responseBody);
            else
                response = BaseResponse<object>.ErrorMessage(code, _localizer[message]);
        }
        else
        {
            response = BaseResponse<object>.ErrorMessage(BaseResponseCode.Unknown.Id, responseBody);
        }

        return response;
    }

    private BaseResponse<object> CreateResponseHttpStatus(int code, string responseBody = null)
    {
        BaseResponse<object> response;
        var message = BaseResponseCode.GetValue<BaseResponseCode>(code);
        response = BaseResponse<object>.ErrorMessage(code, _localizer[message]);
        if (responseBody != null)
            response.Message = responseBody;
        return response;
    }

    private BaseResponse<object> CreateResponseSuccess(string responseBody)
    {
        try
        {
            object data = null;
            if (!string.IsNullOrEmpty(responseBody))
                data = JsonConvert.DeserializeObject<object>(responseBody);

            return BaseResponse<object>.SuccessMessage(data, _localizer[BaseResponseCode.OK.Name]);
        }
        catch (System.Exception)
        {
            return BaseResponse<object>.SuccessMessage(responseBody, _localizer[BaseResponseCode.OK.Name]);
        }
    }

    private BaseResponse CreateResponseException(Exception exception)
    {
        BaseResponse response = BaseResponse<object>.ErrorMessage(exception.HResult, exception.Message);

        var description = $"Exception: {exception.Message} | ExceptionDetail: {exception.StackTrace}";
        if (exception.InnerException != null)
            description += $" | InnerException: {exception.InnerException.Message} | InnerExceptionDetail: {exception.InnerException.StackTrace}";

        response.Description = description;
        return response;
    }

}