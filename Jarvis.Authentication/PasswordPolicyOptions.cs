namespace Jarvis.Authentication;

public class PasswordPolicyOptions
{
    public int MinLength { get; set; } = 8;

    public bool RequireDigit { get; set; }

    public bool RequireUppercase { get; set; }

    public bool RequireLowercase { get; set; }

    public bool RequireNonAlphanumeric { get; set; }

    public int MaxFailedAttempts { get; set; }
}
