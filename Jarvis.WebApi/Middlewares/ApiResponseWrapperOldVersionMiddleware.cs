using System.Diagnostics;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Jarvis.Shared;
using Jarvis.Shared.Constants;
using Jarvis.Shared.Enums;
using Jarvis.Shared.Extensions;

namespace Jarvis.WebApi.Middlewares;

public class ApiResponseWrapperOldVersionMiddleware
{
    private readonly IStringLocalizer<ApiResponseWrapperOldVersionMiddleware> _localizer;
    private readonly ILogger<ApiResponseWrapperOldVersionMiddleware> _logger;
    private readonly RequestDelegate _next;

    private readonly JsonSerializerSettings JsonOptions = new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    };

    public ApiResponseWrapperOldVersionMiddleware(
        IStringLocalizer<ApiResponseWrapperOldVersionMiddleware> localizer,
        ILogger<ApiResponseWrapperOldVersionMiddleware> logger,
        RequestDelegate next)
    {
        _localizer = localizer;
        _logger = logger;
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.IsSwagger("/swagger") || context.Request.IsExclude("/healthz"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.ContentType.AllowContentType(ContentType.Json, ContentType.Xml, ContentType.Text))
        {
            await _next(context);
            return;
        }

        var stopWatch = Stopwatch.StartNew();
        var requestBody = await context.Request.GetRequestBodyAsync();
        var originalResponseBodyStream = context.Response.Body;
        var headers = new string[] { "Authorization", "X-API-KEY", "X-API-RESPONSE-VERSION" };

        using (var stream = new MemoryStream())
        {
            try
            {
                context.Response.Body = stream;
                await _next(context);

                if (context.Response.HasStarted)
                    return;

                if (context.Response.ContentType == ContentType.Stream)
                {
                    stream.Position = 0;
                    string responseBody = new StreamReader(stream).ReadToEnd();

                    stream.Position = 0;
                    await stream.CopyToAsync(originalResponseBodyStream);
                }
                else
                {
                    var responseBody = await HttpContextExtension.ReadResponseBodyStreamAsync(stream);
                    context.Response.Body = originalResponseBodyStream;

                    string json = GenerateContentSuccess(context, responseBody);
                    context.Response.ContentType = ContentType.Json;
                    context.Response.ContentLength = Encoding.UTF8.GetByteCount(json);
                    await context.Response.WriteAsync(json);
                }
            }
            catch (Exception ex)
            {
                if (context.Response.HasStarted)
                    return;

                _logger.LogError(ex, ex.Message);

                var json = GenerateContentError(context, ex);
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = ContentType.Json;
                context.Response.ContentLength = Encoding.UTF8.GetByteCount(json);
                await context.Response.WriteAsync(json);

                stream.Seek(0, SeekOrigin.Begin);
                await stream.CopyToAsync(originalResponseBodyStream);
            }
            finally
            {
                stopWatch.Stop();

                _logger.LogInformation(
                    $"Source:[{context.Connection.RemoteIpAddress}]"
                    + $" | Request: {context.Request.Method} {context.Request.Scheme} {context.Request.Host}{context.Request.Path} {context.Request.QueryString}"
                    + $" | Request Headers: {string.Join(" _ ", context.Request.Headers.Where(x => headers.Contains(x.Key)).Select(x => $"[{x.Key}]:[{x.Value}]"))}"
                    + $" | Status Code: {context.Response.StatusCode}"
                    + $" | Response Time: {stopWatch.ElapsedMilliseconds}ms"
                    + $" | Body: {requestBody}");
            }
        }
    }

    private string GenerateContentError(HttpContext context, Exception ex)
    {
        if (!context.Request.Headers.TryGetValue("X-API-RESPONSE-VERSION", out StringValues version))
            return JsonConvert.SerializeObject(new ResponseV1(_localizer).CreateResponseException(ex), JsonOptions);

        if (version == 1)
            return JsonConvert.SerializeObject(new ResponseV1(_localizer).CreateResponseException(ex), JsonOptions);

        return JsonConvert.SerializeObject(new ResponseV2(_localizer).CreateResponseException(ex), JsonOptions);
    }

    private string GenerateContentSuccess(HttpContext context, string responseBody)
    {
        if (!context.Request.Headers.TryGetValue("X-API-RESPONSE-VERSION", out StringValues version))
            return JsonConvert.SerializeObject(new ResponseV1(_localizer).GenerateResponse(context, responseBody), JsonOptions);

        if (version == 1)
            return JsonConvert.SerializeObject(new ResponseV1(_localizer).GenerateResponse(context, responseBody), JsonOptions);

        return JsonConvert.SerializeObject(new ResponseV2(_localizer).GenerateResponse(context, responseBody), JsonOptions);
    }

    private class ResponseV2
    {
        private readonly IStringLocalizer<ApiResponseWrapperOldVersionMiddleware> _localizer;

        public ResponseV2(IStringLocalizer<ApiResponseWrapperOldVersionMiddleware> localizer)
        {
            _localizer = localizer;
        }

        public BaseResponse<object> GenerateResponse(HttpContext context, string responseBody)
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

        public BaseResponse<object> CreateResponseError(string responseBody)
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

        public BaseResponse<object> CreateResponseHttpStatus(int code, string responseBody = null)
        {
            BaseResponse<object> response;
            var message = BaseResponseCode.GetValue<BaseResponseCode>(code);
            response = BaseResponse<object>.ErrorMessage(code, _localizer[message]);
            if (responseBody != null)
                response.Message = responseBody;
            return response;
        }

        public BaseResponse<object> CreateResponseSuccess(string responseBody)
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

        public BaseResponse CreateResponseException(Exception exception)
        {
            BaseResponse response = BaseResponse<object>.ErrorMessage(exception.HResult, exception.Message);

            var description = $"Exception: {exception.Message} | ExceptionDetail: {exception.StackTrace}";
            if (exception.InnerException != null)
                description += $" | InnerException: {exception.InnerException.Message} | InnerExceptionDetail: {exception.InnerException.StackTrace}";

            response.Description = description;
            return response;
        }
    }

    private class ResponseV1
    {
        private readonly IStringLocalizer<ApiResponseWrapperOldVersionMiddleware> _localizer;

        public ResponseV1(IStringLocalizer<ApiResponseWrapperOldVersionMiddleware> localizer)
        {
            _localizer = localizer;
        }

        public BaseResponseOldVersion<object> GenerateResponse(HttpContext context, string responseBody)
        {
            BaseResponseOldVersion<object> response;
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

        public BaseResponseOldVersion<object> CreateResponseError(string responseBody)
        {
            BaseResponseOldVersion<object> response;
            if (int.TryParse(responseBody, out int code))
            {

                var message = BaseResponseCode.GetValue<BaseResponseCode>(code);
                if (message == null)
                    response = new BaseResponseOldVersion<object>(false, code, responseBody, null);
                else
                    response = new BaseResponseOldVersion<object>(false, code, _localizer[message], null);
            }
            else
            {
                response = new BaseResponseOldVersion<object>(false, BaseResponseCode.Unknown.Id, responseBody, null);
            }

            return response;
        }

        public BaseResponseOldVersion<object> CreateResponseHttpStatus(int code, string responseBody = null)
        {
            BaseResponseOldVersion<object> response;
            var message = BaseResponseCode.GetValue<BaseResponseCode>(code);
            response = new BaseResponseOldVersion<object>(false, code, _localizer[message], null);
            if (responseBody != null)
                response.Message = responseBody;
            return response;
        }

        public BaseResponseOldVersion<object> CreateResponseSuccess(string responseBody)
        {
            try
            {
                object data = null;
                if (string.IsNullOrEmpty(responseBody))
                    return new BaseResponseOldVersion<object>(true, BaseResponseCode.OK.Id, _localizer[BaseResponseCode.OK.Name], data);

                if (DateTime.TryParse(responseBody, out DateTime datetimeValue))
                    return new BaseResponseOldVersion<object>(true, BaseResponseCode.OK.Id, _localizer[BaseResponseCode.OK.Name], datetimeValue);

                if (int.TryParse(responseBody, out int intValue))
                    return new BaseResponseOldVersion<object>(true, BaseResponseCode.OK.Id, _localizer[BaseResponseCode.OK.Name], intValue);

                if (long.TryParse(responseBody, out long longValue))
                    return new BaseResponseOldVersion<object>(true, BaseResponseCode.OK.Id, _localizer[BaseResponseCode.OK.Name], longValue);

                if (bool.TryParse(responseBody, out bool boolValue))
                    return new BaseResponseOldVersion<object>(true, BaseResponseCode.OK.Id, _localizer[BaseResponseCode.OK.Name], boolValue);

                data = JsonConvert.DeserializeObject<object>(responseBody);

                return new BaseResponseOldVersion<object>(true, BaseResponseCode.OK.Id, _localizer[BaseResponseCode.OK.Name], data);
            }
            catch (System.Exception)
            {
                return new BaseResponseOldVersion<object>(true, BaseResponseCode.OK.Id, _localizer[BaseResponseCode.OK.Name], responseBody);
            }
        }

        public BaseResponseOldVersion CreateResponseException(Exception exception)
        {

            var description = $"Exception: {exception.Message} | ExceptionDetail: {exception.StackTrace}";
            if (exception.InnerException != null)
                description += $" | InnerException: {exception.InnerException.Message} | InnerExceptionDetail: {exception.InnerException.StackTrace}";

            return new BaseResponseOldVersion<object>(false, exception.HResult, exception.Message, null, description);
        }
    }

}