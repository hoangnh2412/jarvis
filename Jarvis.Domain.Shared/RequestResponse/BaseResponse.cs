using System.Net;
using System.Reflection;

namespace Jarvis.Domain.Shared.RequestResponse;

#nullable disable

public class BaseResponse
{
    /// <summary>
    /// Unique request identifier
    /// </summary>
    public string RequestId { get; set; }

    /// <summary>
    /// Timestamp of request. Timezone UTC
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Error code. Structure: {NAME}:{XXX}{YY}000
    /// - NAME: Name of project. Ex: Sample.WebApi
    /// - XXX: HttpStatusCode of request. Ex: 200, 202, 400, 401, 403, 404, 409, 422, 500
    /// - YY: Identity of resource. Ex: User = 01, ShippingAddress = 02
    /// - 000: Error code, default is 999
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// List of error codees by field in case HttpStatusCode = 400
    /// Sample: { "fieldName": [ "XXXYY001", "XXXYY002" ] }
    /// </summary>
    public BaseResponseError Error { get; set; }

    public BaseResponse() { }

    public BaseResponse(string requestId, HttpStatusCode httpStatusCode, string code, BaseResponseError error = null)
    {
        RequestId = requestId;
        Timestamp = DateTime.UtcNow;
        Code = GenerateCode(httpStatusCode, code);
        Error = error;
    }

    public static string GenerateCode(HttpStatusCode httpStatusCode, string code)
    {
        var assemblyName = Assembly.GetEntryAssembly().GetName().Name;
        if (code.StartsWith(assemblyName))
            return code;

        return $"{assemblyName}:{httpStatusCode.GetHashCode()}{code}";
    }
}

public class BaseResponse<T> : BaseResponse
{
    /// <summary>
    /// Response data
    /// </summary>
    /// <value></value>
    public T Data { get; set; }

    public BaseResponse() { }

    public BaseResponse(
        string requestId,
        HttpStatusCode httpStatusCode,
        string code,
        BaseResponseError error = null)
        : base(requestId, httpStatusCode, code, error)
    {
        RequestId = requestId;
        Timestamp = DateTime.UtcNow;
        Code = GenerateCode(httpStatusCode, code);
        Error = error;
    }

    public BaseResponse(
        string requestId,
        HttpStatusCode httpStatusCode,
        string code,
        T data,
        BaseResponseError error = null)
        : base(requestId, httpStatusCode, code, error)
    {
        RequestId = requestId;
        Timestamp = DateTime.UtcNow;
        Code = GenerateCode(httpStatusCode, code);
        Error = error;
        Data = data;
    }
}