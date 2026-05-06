using System.Net;
using System.Text;
using Jarvis.Common.Constants;
using Jarvis.Domain.Shared.Enums;
using Jarvis.Domain.Shared.ExceptionHandling;
using Jarvis.Domain.Shared.RequestResponse;
using Jarvis.Mvc.Extensions;
using Jarvis.OpenTelemetry.SemanticConvention;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Jarvis.Mvc.ExceptionHandling;

public class ApiResponseWrapperMiddleware(
    IOptions<MvcNewtonsoftJsonOptions> jsonOption,
    ILogger<ApiResponseWrapperMiddleware> logger,
    RequestDelegate next)
{
    private readonly MvcNewtonsoftJsonOptions _jsonOption = jsonOption.Value;
    private readonly ILogger<ApiResponseWrapperMiddleware> _logger = logger;
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var requestBody = await context.Request.GetRequestBodyAsync();
        var originalResponseBodyStream = context.Response.Body;

        using var stream = new MemoryStream();

        try
        {
            context.Response.Body = stream;
            await _next(context);

            if (context.Response.HasStarted)
                return;

            var responseBody = await HttpContextExtension.ReadResponseBodyStreamAsync(stream);
            context.Response.Body = originalResponseBodyStream;
            context.Response.ContentType = ContentTypes.Json;

            var responseContent = GenerateResponseContent(context, responseBody);
            context.Response.ContentLength = Encoding.UTF8.GetByteCount(responseContent);
            await context.Response.WriteAsync(responseContent);
        }
        catch (Exception ex)
        {
            if (ex is BusinessException businessException)
            {
                if (businessException.InnerException is null)
                {
                    _logger.LogError(businessException, $"{ExceptionAttributes.Source}.{ExceptionAttributes.Type}: [{ExceptionAttributes.Code}] {ExceptionAttributes.Message} - {ExceptionAttributes.SystemMessage}\n{ExceptionAttributes.StackTrace}",
                        businessException.Source,
                        businessException.GetType().Name,
                        businessException.Code,
                        businessException.Message,
                        businessException.SystemMessage,
                        businessException.StackTrace);
                }
                else
                {
                    _logger.LogError(businessException, $"{ExceptionAttributes.Source}.{ExceptionAttributes.Type}: [{ExceptionAttributes.Code}] {ExceptionAttributes.Message} - {ExceptionAttributes.SystemMessage}\n{ExceptionAttributes.StackTrace}\nInnerException: {ExceptionAttributes.InnerSource}.{ExceptionAttributes.InnerType}: [{ExceptionAttributes.InnerCode}] {ExceptionAttributes.InnerMessage}\n{ExceptionAttributes.InnerStackTrace}",
                        businessException.Source,
                        businessException.GetType().Name,
                        businessException.Code,
                        businessException.Message,
                        businessException.SystemMessage,
                        businessException.StackTrace,
                        businessException.InnerException.Source,
                        businessException.InnerException.GetType().Name,
                        businessException.InnerException.HResult,
                        businessException.InnerException.Message,
                        businessException.InnerException.StackTrace);
                }
            }
            else
            {
                if (ex.InnerException is null)
                {
                    _logger.LogError(ex, $"{ExceptionAttributes.Source}.{ExceptionAttributes.Type}: {ExceptionAttributes.Message}\n{ExceptionAttributes.StackTrace}",
                        ex.Source,
                        ex.GetType().Name,
                        ex.Message,
                        ex.StackTrace);
                }
                else
                {
                    _logger.LogError(ex, $"{ExceptionAttributes.Source}.{ExceptionAttributes.Type}: {ExceptionAttributes.Message}\n{ExceptionAttributes.StackTrace}\nnInnerException: {ExceptionAttributes.InnerSource}.{ExceptionAttributes.InnerType}: [{ExceptionAttributes.InnerCode}] {ExceptionAttributes.InnerMessage}\n{ExceptionAttributes.InnerStackTrace}",
                        ex.Source,
                        ex.GetType().Name,
                        ex.Message,
                        ex.StackTrace,
                        ex.InnerException.Source,
                        ex.InnerException.GetType().Name,
                        ex.InnerException.HResult,
                        ex.InnerException.Message,
                        ex.InnerException.StackTrace);
                }
            }

            if (context.Response.HasStarted)
                return;

            var responseContent = GenerateResponseContent(context, ex);
            var responseBody = JsonConvert.SerializeObject(responseContent.Item1, _jsonOption.SerializerSettings);
            context.Response.StatusCode = responseContent.Item2.GetHashCode();
            context.Response.ContentLength = Encoding.UTF8.GetByteCount(responseBody);
            context.Response.ContentType = ContentTypes.Json;
            context.Response.Body = originalResponseBodyStream;
            await context.Response.WriteAsync(responseBody);
        }
    }

    private static (BaseResponse, HttpStatusCode) GenerateResponseContent(HttpContext context, Exception ex)
    {
        var (response, httpStatusCode) = (new BaseResponse(BaseErrorCode.Error, new BaseResponseError(ex.Message, null)), HttpStatusCode.InternalServerError);

        if (ex is BusinessException)
            (response, httpStatusCode) = ParseStatusCode<BusinessException>(ex);

        if (ex is ConflictException)
            (response, httpStatusCode) = ParseStatusCode<ConflictException>(ex);

        if (ex is NotFoundException)
            (response, httpStatusCode) = ParseStatusCode<NotFoundException>(ex);

        if (ex is BadRequestException)
            (response, httpStatusCode) = ParseStatusCode<BadRequestException>(ex);

        if (ex is ForbiddenException)
            (response, httpStatusCode) = ParseStatusCode<ForbiddenException>(ex);

        if (ex is UnauthorizedException)
            (response, httpStatusCode) = ParseStatusCode<UnauthorizedException>(ex);

        // if (ex is RateLimitedException rlEx)
        // {
        //     context.Response.Headers[HeaderConstants.RateLimitLimit] = rlEx.Limit.ToString();
        //     context.Response.Headers[HeaderConstants.RateLimitReset] = rlEx.RetryAfter.ToString();
        //     context.Response.Headers[HeaderConstants.RateLimitRemaining] = "0";
        //     (response, httpStatusCode) = ParseStatusCode<RateLimitedException>(rlEx);
        // }

        return (response, httpStatusCode);
    }

    private static (BaseResponse, HttpStatusCode) ParseStatusCode<T>(Exception ex) where T : BusinessException
    {
        if (ex is T businessException)
        {
            if (businessException.Content == null)
                return (new BaseResponse(businessException.Code, new BaseResponseError(businessException.Message, businessException.SystemMessage)), businessException.HttpStatusCode);

            return (new BaseResponse<object>(businessException.Code, businessException.Content, new BaseResponseError(businessException.Message, businessException.SystemMessage)), businessException.HttpStatusCode);
        }

        return (new BaseResponse(BaseErrorCode.Error, new BaseResponseError(ex.Message, null)), HttpStatusCode.InternalServerError);
    }

    private string GenerateResponseContent(HttpContext context, string content)
    {
        if (context.Response.StatusCode == HttpStatusCode.OK.GetHashCode())
            return ParseSuccessContent(context, content);

        if (context.Response.StatusCode == HttpStatusCode.BadRequest.GetHashCode())
            return ParseBadRequest(context, content);

        var statusCodes = new int[] { HttpStatusCode.Unauthorized.GetHashCode(), HttpStatusCode.Forbidden.GetHashCode(), HttpStatusCode.NotFound.GetHashCode() };
        if (statusCodes.Contains(context.Response.StatusCode))
            return ParseErrorContent(context, content, BaseErrorCode.Error);

        return content;
    }

    private string ParseSuccessContent(HttpContext context, string content)
    {
        var response = new BaseResponse<object>(BaseErrorCode.OK);

        if (!string.IsNullOrEmpty(content))
            response.Data = JsonConvert.DeserializeObject<object>(content, _jsonOption.SerializerSettings);

        return JsonConvert.SerializeObject(response, _jsonOption.SerializerSettings);
    }

    private string ParseErrorContent(HttpContext context, string content, string defaultCode)
    {
        if (string.IsNullOrEmpty(content))
            content = defaultCode;

        return JsonConvert.SerializeObject(new BaseResponse(content), _jsonOption.SerializerSettings);
    }

    private string ParseBadRequest(HttpContext context, string content)
    {
        var response = new BaseResponse(BaseErrorCode.Error);

        try
        {
            response = JsonConvert.DeserializeObject<BaseResponse>(content, _jsonOption.SerializerSettings);
            if (response == null)
                _logger.LogError("Cannot deserialize: {content}", content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{message}", ex.Message);
        }

        return JsonConvert.SerializeObject(response, _jsonOption.SerializerSettings);
    }
}