namespace Jarvis.Authentication.Basic;

public class AuthenticationBasicOption
{
    public string Realm { get; set; } = "Jarvis API";

    public Dictionary<string, BasicUserCredential> Users { get; set; } = new(StringComparer.Ordinal);
}

public class BasicUserCredential
{
    public string Password { get; set; } = string.Empty;

    public string[] Roles { get; set; } = [];
}
