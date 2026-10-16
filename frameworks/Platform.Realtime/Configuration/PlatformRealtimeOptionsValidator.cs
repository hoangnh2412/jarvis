using Microsoft.Extensions.Options;

namespace Platform.Realtime.Configuration;

public sealed class PlatformRealtimeOptionsValidator : IValidateOptions<PlatformRealtimeOptions>
{
    public ValidateOptionsResult Validate(string? name, PlatformRealtimeOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.SignalR.SignalRPath) && options.SignalR.Enabled)
            return ValidateOptionsResult.Fail("Realtime:SignalR:SignalRPath is required when Realtime:SignalR:Enabled is true.");

        if (string.IsNullOrWhiteSpace(options.Sse.SsePath) && options.Sse.Enabled)
            return ValidateOptionsResult.Fail("Realtime:Sse:SsePath is required when Realtime:Sse:Enabled is true.");

        if (options.SignalR.UseRedisBackplane && string.IsNullOrWhiteSpace(options.Redis.Configuration))
            return ValidateOptionsResult.Fail("Realtime:SignalR:UseRedisBackplane is true but Realtime:Redis:Configuration is not set.");

        if (options.Sse.UseRedisFanOut && string.IsNullOrWhiteSpace(options.Redis.Configuration))
            return ValidateOptionsResult.Fail("Realtime:Sse:UseRedisFanOut is true but Realtime:Redis:Configuration is not set.");

        if (options.Sse.UseRedisFanOut && !options.Sse.Enabled)
            return ValidateOptionsResult.Fail("Realtime:Sse:UseRedisFanOut is true but Realtime:Sse:Enabled is false.");
        
        if (options.Sse.Enabled && options.Sse.HeartbeatSeconds < 1)
            return ValidateOptionsResult.Fail("Realtime:Sse:HeartbeatSeconds must be >= 1.");

        return ValidateOptionsResult.Success;
    }
}
