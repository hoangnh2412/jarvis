using Microsoft.Extensions.Options;

namespace Jarvis.Realtime.Configuration;

public sealed class JarvisRealtimeOptionsValidator : IValidateOptions<JarvisRealtimeOptions>
{
    public ValidateOptionsResult Validate(string? name, JarvisRealtimeOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.HubPath))
            return ValidateOptionsResult.Fail("Realtime:SignalR:HubPath is required.");

        return ValidateOptionsResult.Success;
    }
}
