namespace Platform.Authentication;

/// <summary>Rule validate mật khẩu theo <see cref="PasswordPolicyOptions"/>.</summary>
public static class PasswordPolicyRules
{
    public static PasswordValidationResult Validate(string password, PasswordPolicyOptions policy)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(password) || password.Length < policy.MinLength)
            errors.Add($"Password must be at least {policy.MinLength} characters.");

        if (policy.RequireDigit && !password.Any(char.IsDigit))
            errors.Add("Password must contain a digit.");

        if (policy.RequireUppercase && !password.Any(char.IsUpper))
            errors.Add("Password must contain an uppercase letter.");

        if (policy.RequireLowercase && !password.Any(char.IsLower))
            errors.Add("Password must contain a lowercase letter.");

        if (policy.RequireNonAlphanumeric && password.All(char.IsLetterOrDigit))
            errors.Add("Password must contain a non-alphanumeric character.");

        return errors.Count == 0
            ? PasswordValidationResult.Success()
            : PasswordValidationResult.Failed(errors.ToArray());
    }
}
