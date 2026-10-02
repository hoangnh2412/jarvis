using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Platform.Modules.Setting.Definitions;

namespace Platform.Modules.Setting.Extensions;

/// <summary>
/// Builder fluent sau <see cref="PlatformSettingExtensions.AddCoreSetting(IHostApplicationBuilder)"/>.
/// Cho phép nối tiếp <c>UseEntityFramework</c>, <c>UseHttpApi</c>, <c>AddProvider</c>.
/// </summary>
public sealed class PlatformSettingBuilder
{
    /// <summary>Khởi tạo builder gắn với host hiện tại.</summary>
    internal PlatformSettingBuilder(IHostApplicationBuilder hostBuilder)
    {
        HostBuilder = hostBuilder;
    }

    /// <summary>Host builder gốc — các package mở rộng (EF/Redis) dùng để đăng ký thêm service.</summary>
    public IHostApplicationBuilder HostBuilder { get; }

    /// <summary>
    /// Đăng ký một <see cref="ISettingDefinitionProvider"/> code-first vào DI.
    /// Có thể gọi nhiều lần cho nhiều nhóm nghiệp vụ (Email, SMS…).
    /// </summary>
    /// <typeparam name="TProvider">Class provider của host/module nghiệp vụ.</typeparam>
    /// <returns>Chính builder để gọi fluent tiếp.</returns>
    public PlatformSettingBuilder AddProvider<TProvider>()
        where TProvider : class, ISettingDefinitionProvider
    {
        HostBuilder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<ISettingDefinitionProvider, TProvider>());
        return this;
    }
}
