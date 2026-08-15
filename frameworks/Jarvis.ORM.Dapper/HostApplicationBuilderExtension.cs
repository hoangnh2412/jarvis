using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Jarvis.ORM.Dapper;

/// <summary>
/// DI entry points for <c>Jarvis.ORM.Dapper</c> foundation (connection factory only — MVP).
/// </summary>
public static class HostApplicationBuilderExtension
{
    /// <summary>
    /// Registers <see cref="ISqlConnectionFactory"/> for Dapper read/report usage.
    /// Configure <see cref="OrmDapperOptions.CreateConnection"/> for the ADO.NET provider.
    /// Does not register repositories or UoW — keep this package thin (see ADR Jarvis.ORM.*).
    /// </summary>
    public static IHostApplicationBuilder AddOrmDapper(
        this IHostApplicationBuilder builder,
        Action<OrmDapperOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (configure is not null)
            builder.Services.Configure(configure);
        else
            builder.Services.Configure<OrmDapperOptions>(_ => { });

        builder.Services.TryAddSingleton<ISqlConnectionFactory, ConfigSqlConnectionFactory>();
        return builder;
    }
}
