using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Jarvis.Realtime.Configuration;
using Jarvis.Realtime.Hosting;

namespace Jarvis.Realtime.Extensions;

public static class RealtimeHostBuilderExtensions
{
    /// <summary>
    /// Đăng ký realtime core (options, validation). Tiếp theo gọi satellite <c>UseSignalR</c> / backplane.
    /// </summary>
    public static RealtimeBuilder AddCoreRealtime(
        this IHostApplicationBuilder builder,
        Action<JarvisRealtimeOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddSingleton<IValidateOptions<JarvisRealtimeOptions>, JarvisRealtimeOptionsValidator>();

        builder.Services
            .AddOptions<JarvisRealtimeOptions>()
            .BindConfiguration(JarvisRealtimeOptions.SectionName)
            .ValidateOnStart();

        if (configure is not null)
            builder.Services.Configure(configure);

        var snapshot = new JarvisRealtimeOptions();
        builder.Configuration.GetSection(JarvisRealtimeOptions.SectionName).Bind(snapshot);
        configure?.Invoke(snapshot);

        return new RealtimeBuilder(builder, snapshot);
    }
}
