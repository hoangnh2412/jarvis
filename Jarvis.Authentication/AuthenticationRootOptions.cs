namespace Jarvis.Authentication;

public class AuthenticationRootOptions
{
    public string Type { get; set; } = "Jwt";

    public string? DefaultAuthenticateScheme { get; set; }

    public string? DefaultChallengeScheme { get; set; }

    public AuthenticationSchemesEnableOptions Schemes { get; set; } = new();

    public PasswordPolicyOptions PasswordPolicy { get; set; } = new();

    public PasswordExpirationOptions PasswordExpiration { get; set; } = new();

    public JarvisCookieAuthenticationOptions Cookie { get; set; } = new();
}
