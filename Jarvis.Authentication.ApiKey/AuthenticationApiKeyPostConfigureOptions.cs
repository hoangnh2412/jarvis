using Microsoft.Extensions.Options;

namespace Jarvis.Authentication.ApiKey;

public sealed class AuthenticationApiKeyPostConfigureOptions : IPostConfigureOptions<AuthenticationApiKeyOption>
{
    public void PostConfigure(string? name, AuthenticationApiKeyOption options)
    {
        options.KeySet = options.Keys.Length > 0
            ? new HashSet<string>(options.Keys, StringComparer.Ordinal)
            : new HashSet<string>(StringComparer.Ordinal);
    }
}
