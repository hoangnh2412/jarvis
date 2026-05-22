namespace Jarvis.Authentication;

public class JarvisCookieAuthenticationOptions
{
    public string LoginPath { get; set; } = "/account/login";

    public string LogoutPath { get; set; } = "/account/logout";

    public TimeSpan ExpireTimeSpan { get; set; } = TimeSpan.FromHours(1);

    public bool SlidingExpiration { get; set; } = true;

    public bool HttpOnly { get; set; } = true;

    public string SameSite { get; set; } = "Lax";
}
