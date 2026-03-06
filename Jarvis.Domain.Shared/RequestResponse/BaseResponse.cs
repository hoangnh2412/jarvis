using System.Reflection;

namespace Jarvis.Domain.Shared.RequestResponse;

public class BaseResponse
{
    /// <summary>
    /// Unique trace identifier
    /// </summary>
    public string? TraceId { get; set; }
    public string? SpanId { get; set; }

    /// <summary>
    /// Include traceparent in the next request's header for end-to-end tracing
    /// </summary>
    public string? TraceParent { get; set; }

    /// <summary>
    /// Timestamp of request. Timezone UTC
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Error code. Structure: {NAME}:{YY}00000
    /// - NAME: Name of project. Ex: Sample.User.WebApi
    /// - YY: Identity of resource. Ex: User = 01, ShippingAddress = 02
    /// - 000: Error code, default is 999
    /// Document reference: https://dev.azure.com/belleai/Common%20Library/_wiki/wikis/Common-Library.wiki/3098/Coding-Convention?anchor=error-code
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// List of error codees by field in case HttpStatusCode = 400
    /// Sample: { "fieldName": [ "XXXYY001", "XXXYY002" ] }
    /// </summary>
    public BaseResponseError? Error { get; set; }

    public BaseResponse() { }

    public BaseResponse(string code, BaseResponseError? error = null)
    {
        TraceId = System.Diagnostics.Activity.Current!.TraceId.ToString();
        SpanId = System.Diagnostics.Activity.Current!.SpanId.ToString();
        TraceParent = $"00-{TraceId}-{SpanId}-01";
        Timestamp = DateTime.UtcNow;
        Code = GenerateCode(code);
        Error = error;
    }

    public static string GenerateCode(string code)
    {
        var assemblyName = Assembly.GetEntryAssembly()!.GetName().Name!;
        if (code.StartsWith(assemblyName))
            return code;

        return $"{assemblyName}:{code}";
    }
}

public class BaseResponse<T> : BaseResponse
{
    /// <summary>
    /// Response data
    /// </summary>
    /// <value></value>
    public T? Data { get; set; }

    public BaseResponse() { }

    public BaseResponse(
        string code,
        BaseResponseError? error = null)
        : base(code, error)
    {
        Timestamp = DateTime.UtcNow;
        Code = GenerateCode(code);
        Error = error;
    }

    public BaseResponse(
        string code,
        T data,
        BaseResponseError? error = null)
        : base(code, error)
    {
        Timestamp = DateTime.UtcNow;
        Code = GenerateCode(code);
        Error = error;
        Data = data;
    }
}