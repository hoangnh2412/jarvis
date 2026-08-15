using Jarvis.DDD.Domain.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Jarvis.Authentication;

public static class HostApplicationBuilderExtension
{
    /// <summary>
    /// Đăng ký <see cref="ICurrentUser{TUser}"/> và accessor.
    /// Host bắt buộc đăng ký <see cref="ICurrentUserStore{TUser}"/> (DB/cache/…).
    /// Tenant: <c>AddCurrentTenant&lt;TTenant&gt;()</c> từ <c>Jarvis.Multitenancy</c>.
    /// </summary>
    public static IHostApplicationBuilder AddCurrentUser<TUser>(this IHostApplicationBuilder builder)
        where TUser : class, ICurrentUserIdentity
    {
        builder.Services.TryAddSingleton<ICurrentUserAccessor<TUser>, CurrentUserAccessor<TUser>>();
        builder.Services.TryAddScoped<ICurrentUser<TUser>, CurrentUser<TUser>>();
        return builder;
    }
}
