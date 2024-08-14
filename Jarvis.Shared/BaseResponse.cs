using System.Reflection;
using Jarvis.Shared.Enums;

namespace Jarvis.Shared;

public class BaseResponse
{
    public string Code { get; set; }
    public string Message { get; set; }
    public string Description { get; set; }
    public double Duration { get; set; }
    public ICollection<IDictionary<string, string>> Validation { get; set; }
    public IDictionary<string, object> Parameters { get; set; }

    public BaseResponse(
        int code,
        string message = null,
        double duration = 0,
        ICollection<IDictionary<string, string>> validation = null,
        IDictionary<string, object> parameters = null)
    {
        Code = GenerateCode(code);
        Message = message;
        Duration = duration;
        Validation = validation;
        Parameters = parameters;
    }

    protected static string GenerateCode(int code) => $"{Assembly.GetEntryAssembly().GetName().Name}:{code.ToString("000000")}";

    #region Successes
    public static BaseResponse SuccessMessage() => new BaseResponse(BaseResponseCode.OK.Id);

    public static BaseResponse SuccessMessage(string message) => new BaseResponse(BaseResponseCode.OK.Id, message);
    #endregion

    #region Errors
    public static BaseResponse ErrorMessage() => new BaseResponse(BaseResponseCode.Unknown.Id);

    public static BaseResponse ErrorMessage(string message) => new BaseResponse(BaseResponseCode.Unknown.Id, message);
    #endregion
}

public class BaseResponse<T> : BaseResponse
{
    public T Data { get; set; }

    public BaseResponse(
        int code,
        string message = null,
        T data = default(T),
        double duration = 0,
        ICollection<IDictionary<string, string>> validation = null,
        IDictionary<string, object> parameters = null)
        : base(code, message, duration, validation, parameters)
    {
        Code = GenerateCode(code);
        Message = message;
        Duration = duration;
        Validation = validation;
        Parameters = parameters;
        Data = data;
    }

    #region Successes
    public static BaseResponse<T> SuccessMessage(T data) => new BaseResponse<T>(BaseResponseCode.OK.Id, null, data);

    public static BaseResponse<T> SuccessMessage(T data, string message) => new BaseResponse<T>(BaseResponseCode.OK.Id, message, data);

    public static BaseResponse<T> SuccessMessage(T data, int code) => new BaseResponse<T>(code, null, data);

    public static BaseResponse<T> SuccessMessage(T data, int code, string message) => new BaseResponse<T>(code, message, data);
    #endregion

    #region Errors
    public static new BaseResponse<T> ErrorMessage() => new BaseResponse<T>(BaseResponseCode.Unknown.Id, null, default(T));

    public static new BaseResponse<T> ErrorMessage(string message) => new BaseResponse<T>(BaseResponseCode.Unknown.Id, message, default(T));

    public static BaseResponse<T> ErrorMessage(int code) => new BaseResponse<T>(code, null, default(T));

    public static BaseResponse<T> ErrorMessage(int code, string message) => new BaseResponse<T>(code, message, default(T));
    #endregion
}