namespace Jarvis.Application.ExceptionHandling;

public class BusinessException : Exception
{
    public BusinessException(int code, string message) : base(message)
    {
        HResult = code;
    }
}