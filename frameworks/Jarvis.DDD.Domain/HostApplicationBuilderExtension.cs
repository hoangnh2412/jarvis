using Microsoft.Extensions.Hosting;

namespace Jarvis.DDD.Domain;

public static class HostApplicationBuilderExtension
{
    /// <summary>
    /// Đăng ký core domain (không gồm current user / tenant).
    /// Host: <c>AddCurrentUser&lt;TUser&gt;()</c> + <c>AddCurrentTenant&lt;TTenant&gt;()</c>.
    /// </summary>
    public static IHostApplicationBuilder AddCoreDomain(this IHostApplicationBuilder builder)
    {
        return builder;
    }
}
