namespace Jarvis.Authentication.ApiKey;

public class AuthenticationApiKeyOption
{
    // public string Realm { get; set; } = string.Empty;
    public required string KeyName { get; set; }
    public string[] Keys { get; set; } = [];
}