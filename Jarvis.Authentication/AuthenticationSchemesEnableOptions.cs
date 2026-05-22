namespace Jarvis.Authentication;

public class AuthenticationSchemesEnableOptions
{
    public AuthenticationSchemeEnableOptions Jwt { get; set; } = new();

    public AuthenticationSchemeEnableOptions ApiKey { get; set; } = new();

    public AuthenticationSchemeEnableOptions Basic { get; set; } = new();
}
