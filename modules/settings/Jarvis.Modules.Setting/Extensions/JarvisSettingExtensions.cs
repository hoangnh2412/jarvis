using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Jarvis.Common.Encryption;
using Jarvis.DDD.Domain.Entities;
using Jarvis.DDD.Domain.Repositories;
using Jarvis.DDD.Domain.Services;
using Jarvis.Modules.Setting.Definitions;
using Jarvis.Modules.Setting.Services;

namespace Jarvis.Modules.Setting.Extensions;

/// <summary>
/// Điểm đăng ký DI của module Setting vào host.
/// </summary>
/// <remarks>
/// Cách dùng phổ biến (entity mặc định từ <c>Jarvis.Modules.Setting.EntityFramework</c>):
/// <code>
/// builder.AddCoreSetting()
///     .UseEntityFramework&lt;IMasterUnitOfWork, CurrentTenantInfo&gt;()
///     .UseHttpApi()
///     .AddProvider&lt;EmailSettingDefinition&gt;();
/// </code>
/// Cách dùng entity tùy chỉnh của host:
/// <code>
/// builder.AddCoreSetting&lt;MySetting, IMasterUnitOfWork, CurrentTenantInfo&gt;()
///     .AddProvider&lt;EmailSettingDefinition&gt;();
/// </code>
/// Lưu ý: phải gọi <c>AddJarvisCaching()</c> trước vì Manager phụ thuộc <c>ICacheService</c>.
/// Host phải đăng ký <c>ICurrentTenant&lt;TTenant&gt;</c> (multitenancy).
/// Cấu hình cache item <c>Setting</c> trong <c>Cache:Items</c> (xem README).
/// HTTP API sẵn: reference <c>Jarvis.Modules.Setting.API</c> và gọi <c>UseHttpApi()</c>.
/// </remarks>
public static class JarvisSettingExtensions
{
    /// <summary>
    /// Đăng ký phần lõi: encryption options + Library registry.
    /// Chưa đăng ký <see cref="ISettingManager"/> — cần gọi tiếp
    /// <c>UseEntityFramework&lt;TUnitOfWork, TTenant&gt;</c> hoặc overload generic với entity riêng.
    /// </summary>
    /// <param name="builder">Host application builder.</param>
    /// <returns>Builder fluent để gọi <c>UseEntityFramework</c> / <c>AddProvider</c>.</returns>
    public static JarvisSettingBuilder AddCoreSetting(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        RegisterCore(builder);
        return new JarvisSettingBuilder(builder);
    }

    /// <summary>
    /// Đăng ký đầy đủ module với entity + Unit of Work do host cung cấp.
    /// Dùng khi khách hàng có entity Setting riêng (không dùng package EntityFramework mặc định).
    /// </summary>
    /// <typeparam name="TSetting">Entity concrete implement <see cref="ISettingEntity"/>.</typeparam>
    /// <typeparam name="TUnitOfWork">Unit of Work của host chứa repository Setting.</typeparam>
    /// <typeparam name="TTenant">Profile tenant của host (implement <see cref="ICurrentTenantIdentity"/>).</typeparam>
    /// <param name="builder">Host application builder.</param>
    /// <returns>Builder fluent để đăng ký thêm provider.</returns>
    public static JarvisSettingBuilder AddCoreSetting<TSetting, TUnitOfWork, TTenant>(
        this IHostApplicationBuilder builder)
        where TSetting : class, ISettingEntity
        where TUnitOfWork : class, IUnitOfWork
        where TTenant : class, ICurrentTenantIdentity
    {
        ArgumentNullException.ThrowIfNull(builder);

        RegisterCore(builder);
        builder.Services.TryAddScoped<ISettingManager, SettingManager<TSetting, TUnitOfWork, TTenant>>();

        return new JarvisSettingBuilder(builder);
    }

    /// <summary>
    /// Đăng ký encryption options (Jarvis.Common) + Library registry.
    /// Mã hóa secret dùng <see cref="AesGcmStringEncryptionHelper"/> (không DI).
    /// Cache item cố định tên <c>Setting</c> trong <c>Cache:Items</c>.
    /// </summary>
    private static void RegisterCore(IHostApplicationBuilder builder)
    {
        builder.Services.AddEncryptionOptions();

        // Registry đọc mọi ISettingDefinitionProvider đã đăng ký (AddProvider) lúc resolve lần đầu.
        builder.Services.TryAddSingleton<ISettingDefinitionRegistry>(sp =>
            new SettingDefinitionRegistry(sp.GetServices<ISettingDefinitionProvider>()));
    }
}
