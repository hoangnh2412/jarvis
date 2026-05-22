namespace Jarvis.Authentication.ApiKey;

public class AuthenticationApiKeyOption
{
    public required string KeyName { get; set; }

    public ApiKeyMode Mode { get; set; } = ApiKeyMode.SingleKey;

    public string[] Keys { get; set; } = [];

    internal HashSet<string> KeySet { get; set; } = new(StringComparer.Ordinal);
}
