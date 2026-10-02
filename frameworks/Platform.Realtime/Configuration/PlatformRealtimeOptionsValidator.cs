using Microsoft.Extensions.Options;

namespace Platform.Realtime.Configuration;

public sealed class PlatformRealtimeOptionsValidator : IValidateOptions<PlatformRealtimeOptions>
{
    public ValidateOptionsResult Validate(string? name, PlatformRealtimeOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.HubPath))
            return ValidateOptionsResult.Fail("Realtime:SignalR:HubPath is required.");

        return ValidateOptionsResult.Success;
    }
}
