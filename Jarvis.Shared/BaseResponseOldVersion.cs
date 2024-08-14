using System.Reflection;
using Jarvis.Shared.Enums;

namespace Jarvis.Shared;

public class BaseResponseOldVersion
{
    public bool Success { get; set; }

    public string Message { get; set; }

    public string ErrorCode { get; set; }

    public string ExceptionMessage { get; set; }

    public BaseResponseOldVersion(
        bool isSuccess,
        int code,
        string message = null,
        string description = null)
    {
        Success = isSuccess;
        ErrorCode = GenerateCode(code);
        Message = message;
        ExceptionMessage = description;
    }

    protected static string GenerateCode(int code) => $"{Assembly.GetEntryAssembly().GetName().Name}:{code.ToString("000000")}";

    #region Successes
    public static BaseResponseOldVersion SuccessMessage() => new BaseResponseOldVersion(true, BaseResponseCode.OK.Id);

    public static BaseResponseOldVersion SuccessMessage(string message) => new BaseResponseOldVersion(true, BaseResponseCode.OK.Id, message);
    #endregion

    #region Errors
    public static BaseResponseOldVersion ErrorMessage() => new BaseResponseOldVersion(true, BaseResponseCode.Unknown.Id);

    public static BaseResponseOldVersion ErrorMessage(string message) => new BaseResponseOldVersion(true, BaseResponseCode.Unknown.Id, message);
    #endregion
}

public class BaseResponseOldVersion<T> : BaseResponseOldVersion
{
    public T Data { get; set; }

    public BaseResponseOldVersion(
        bool isSuccess,
        int code,
        string message = null,
        T data = default(T),
        string description = null)
        : base(isSuccess, code, message, description)
    {
        Success = isSuccess;
        ErrorCode = GenerateCode(code);
        Message = message;
        ExceptionMessage = description;
        Data = data;
    }

    #region Successes
    public static BaseResponseOldVersion SuccessMessage(T data) => new BaseResponseOldVersion(true, BaseResponseCode.OK.Id);

    public static BaseResponseOldVersion SuccessMessage(T data, string message) => new BaseResponseOldVersion(true, BaseResponseCode.OK.Id, message);
    #endregion

    #region Errors
    #endregion
}