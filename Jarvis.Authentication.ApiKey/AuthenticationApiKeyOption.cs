namespace Jarvis.Authentication.ApiKey;

public class AuthenticationApiKeyOption
{
    public string Realm { get; set; } = string.Empty;
    public string KeyName { get; set; } = string.Empty;
    public string[] Keys { get; set; } = [];
}