using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Platform.Realtime.Contracts;
using Platform.Realtime.Hosting;
using Platform.Realtime.Services;

namespace Platform.Realtime.Extensions;

public static class RealtimeBuilderCompositeExtensions
{
    /// <summary>
    /// Đăng ký <see cref="CompositeRealtimeNotifier"/> là implementation duy nhất của <see cref="IRealtimeNotifier"/>.
    /// </summary>
    public static RealtimeBuilder EnsureCompositeRealtimeNotifier(this RealtimeBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.HostBuilder.Services.TryAddSingleton<IRealtimeNotifier, CompositeRealtimeNotifier>();
        return builder;
    }
}
