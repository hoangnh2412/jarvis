namespace Jarvis.Authentication;

/// <summary>Kết quả validate mật khẩu — dùng bởi <see cref="IPasswordPolicyValidator"/>.</summary>
public sealed class PasswordValidationResult
{
    public bool Succeeded { get; init; }

    public IReadOnlyList<string> Errors { get; init; } = [];

    public static PasswordValidationResult Success() => new() { Succeeded = true };

    public static PasswordValidationResult Failed(params string[] errors) => new()
    {
        Succeeded = false,
        Errors = errors
    };
}
