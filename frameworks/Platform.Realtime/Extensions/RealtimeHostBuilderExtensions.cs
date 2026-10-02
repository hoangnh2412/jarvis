using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Platform.Realtime.Configuration;
using Platform.Realtime.Hosting;

namespace Platform.Realtime.Extensions;

public static class RealtimeHostBuilderExtensions
{
    /// <summary>
    /// Đăng ký realtime core (options, validation). Tiếp theo gọi satellite <c>UseSignalR</c> / backplane.
    /// </summary>
    public static RealtimeBuilder AddCoreRealtime(
        this IHostApplicationBuilder builder,
        Action<PlatformRealtimeOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddSingleton<IValidateOptions<PlatformRealtimeOptions>, PlatformRealtimeOptionsValidator>();

        builder.Services
            .AddOptions<PlatformRealtimeOptions>()
            .BindConfiguration(PlatformRealtimeOptions.SectionName)
            .ValidateOnStart();

        if (configure is not null)
            builder.Services.Configure(configure);

        var snapshot = new PlatformRealtimeOptions();
        builder.Configuration.GetSection(PlatformRealtimeOptions.SectionName).Bind(snapshot);
        configure?.Invoke(snapshot);

        return new RealtimeBuilder(builder, snapshot);
    }
}
