namespace Infrastructure.Abstractions.Validations
{
    public class ValidationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        
        public ValidationResult(bool success) {
            Success = success;
        }

        public ValidationResult(bool success, string message) {
            Success = success;
            Message = message;
        }
    }
}