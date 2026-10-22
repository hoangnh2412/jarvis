namespace Platform.Authorization.Services;

/// <summary>Kết quả của kiểm tra xác thực operand / rule.</summary>
public sealed class ValidationResult
{
    public bool IsValid { get; private set; }

    public string? ErrorMessage { get; private set; }

    public static ValidationResult Success() => new() { IsValid = true };

    public static ValidationResult Fail(string message) => new() { IsValid = false, ErrorMessage = message };
}
