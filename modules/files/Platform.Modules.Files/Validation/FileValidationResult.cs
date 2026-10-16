namespace Platform.Modules.Files.Validation;

/// <summary>Result of <see cref="FilePathValidator"/> checks (no HTTP dependency).</summary>
public readonly record struct FileValidationResult
{
    public string? Code { get; init; }

    public string? Message { get; init; }

    public bool IsValid => Code is null;

    public static FileValidationResult Success() => default;

    public static FileValidationResult Fail(string code, string? message = null) => new()
    {
        Code = code,
        Message = message ?? FileErrorCode.Message(code)
    };
}
