namespace Jarvis.Core.Errors
{
    public class ErrorInfo
    {
        public int Code { get; set; }
        public string Message { get; set; }

        public ErrorInfo() { }

        public ErrorInfo(int code, string message)
        {
            Code = code;
            Message = message;
        }
    }
}
