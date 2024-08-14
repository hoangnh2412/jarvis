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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Jarvis.Shared.Options;
using Microsoft.Extensions.Options;

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

    private readonly List<string> _excludePaths = new List<string> { "GET /" };

    private readonly List<string> _ignoreWrapResponses = new List<string> { "GET /" };

    private readonly Dictionary<string, string> _responseTypes = new Dictionary<string, string>();

    private List<string> _allowContentTypes = new List<string> { ContentType.Json, ContentType.Xml };

    public ApiResponseWrapperMiddleware(
        IStringLocalizer<ApiResponseWrapperMiddleware> localizer,
        ILogger<ApiResponseWrapperMiddleware> logger,
        IConfiguration configuration,
        RequestDelegate next)
    {
        _localizer = localizer;
        _logger = logger;
        _next = next;

        BuildOptions(configuration);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.IsSwagger("/swagger") || context.Request.IsExclude(_excludePaths.ToArray()))
        {
            await _next(context);
            return;
        }

        if (!context.Request.ContentType.AllowContentType(_allowContentTypes.ToArray()))
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

                var responseContentType = GetResponseContentType(context);

                var responseContent = responseBody;
                if (!context.Request.IsIgnoreWrapResponse(_ignoreWrapResponses.ToArray()))
                {
                    var response = GenerateResponse(context, responseBody);

                    if (responseContentType == ContentType.Xml)
                        responseContent = XmlExtension.XmlSerialize(response);
                    else
                        responseContent = JsonConvert.SerializeObject(response, JsonOptions);
                }

                context.Response.ContentType = responseContentType;
                context.Response.ContentLength = Encoding.UTF8.GetByteCount(responseContent);
                await context.Response.WriteAsync(responseContent);
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

    private string GetResponseContentType(HttpContext context)
    {
        var responseContentType = ContentType.Json;
        if (_responseTypes.TryGetValue($"{context.Request.Method} {context.Request.Path}", out string responseType))
            responseContentType = responseType;
        return responseContentType;
    }

    private MiddlewareOption BuildOptions(IConfiguration configuration)
    {
        var options = new MiddlewareOption();
        configuration.GetSection($"Middlewares:{nameof(ApiResponseWrapperMiddleware)}").Bind(options);

        if (options.AllowContentTypes != null && options.AllowContentTypes.Count > 0)
            _allowContentTypes = options.AllowContentTypes;

        foreach (var path in options.Paths)
        {
            if (path.Value.TryGetValue("IsExclude", out string isExclude) && bool.Parse(isExclude))
                _excludePaths.Add(path.Key);

            if (path.Value.TryGetValue("IsIgnoreWrapResponse", out string isIgnoreWrapResponse) && bool.Parse(isIgnoreWrapResponse))
                _ignoreWrapResponses.Add(path.Key);

            if (path.Value.TryGetValue("ResponseType", out string responseType) && !string.IsNullOrEmpty(responseType))
                _responseTypes[path.Key] = responseType;
        }

        return options;
    }

    private BaseResponse<object> GenerateResponse(HttpContext context, string responseBody)
    {
        BaseResponse<object> response;
        if (context.Response.StatusCode == HttpStatusCode.OK.GetHashCode())
        {
            response = CreateResponseSuccess(context, responseBody);
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

    private BaseResponse<object> CreateResponseSuccess(HttpContext context, string responseBody)
    {
        try
        {
            object data = null;
            if (!string.IsNullOrEmpty(responseBody))
            {
                var responseContentType = GetResponseContentType(context);
                if (responseContentType == ContentType.Xml)
                    data = XmlExtension.XmlDeserialize<object>(responseBody);
                else
                    data = JsonConvert.DeserializeObject<object>(responseBody);
            }

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