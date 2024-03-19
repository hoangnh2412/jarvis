namespace Jarvis.Application.Validations;

public class ValidationResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; }

    public ValidationResult(bool success)
    {
        IsSuccess = success;
    }

    public ValidationResult(bool success, string message)
    {
        IsSuccess = success;
        Message = message;
    }
}