namespace Platform.Modules.ImportExport.Domain.Exceptions;

public abstract class ExcelEngineException : Exception
{
    protected ExcelEngineException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    protected ExcelEngineException(string errorCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    public string ErrorCode { get; }
}
