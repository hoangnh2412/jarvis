using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Jarvis.DDD.Domain.Repositories;
using Jarvis.DDD.Domain.Services;
using Jarvis.Modules.Setting.Extensions;
using Jarvis.Modules.Setting.Services;
using SettingEntity = Jarvis.Modules.Setting.EntityFramework.Entities.Setting;

namespace Jarvis.Modules.Setting.EntityFramework.Extensions;

/// <summary>
/// Đăng ký entity Setting mặc định với <see cref="ISettingManager"/> qua Entity Framework.
/// </summary>
public static class SettingEntityFrameworkExtensions
{
    /// <summary>
    /// Dùng entity <see cref="SettingEntity"/> đi kèm package và đăng ký
    /// <see cref="SettingManager{TSetting,TUnitOfWork,TTenant}"/> cho Unit of Work của host.
    /// Host cần entity riêng: gọi <c>AddCoreSetting&lt;TSetting, TUnitOfWork, TTenant&gt;</c>
    /// và tự map EF thay vì dùng method này.
    /// </summary>
    /// <typeparam name="TUnitOfWork">Unit of Work host chứa repository Setting.</typeparam>
    /// <typeparam name="TTenant">Profile tenant của host (implement <see cref="ICurrentTenantIdentity"/>).</typeparam>
    /// <param name="builder">Builder fluent sau <c>AddCoreSetting()</c>.</param>
    /// <returns>Chính builder để gọi fluent tiếp (<c>UseHttpApi</c>, <c>AddProvider</c>…).</returns>
    public static JarvisSettingBuilder UseEntityFramework<TUnitOfWork, TTenant>(this JarvisSettingBuilder builder)
        where TUnitOfWork : class, IUnitOfWork
        where TTenant : class, ICurrentTenantIdentity
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HostBuilder.Services.TryAddScoped<ISettingManager, SettingManager<SettingEntity, TUnitOfWork, TTenant>>();
        return builder;
    }
}
