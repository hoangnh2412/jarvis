using Microsoft.Extensions.DependencyInjection;
using Platform.DDD.Domain.Services;
using Platform.Orgs;
using Platform.Orgs.Api.Controllers;
using Platform.Orgs.Extensions;

namespace Platform.Orgs.Api.Extensions;

/// <summary>Đăng ký HTTP API catalog Org (opt-in).</summary>
public static class PlatformOrgsApiExtensions
{
    /// <summary>Expose <see cref="OrgController"/> tại <c>api/v{version}/orgs</c>.</summary>
    public static PlatformOrgsBuilder<TOrg, TTenant> UseHttpApi<TOrg, TTenant>(
        this PlatformOrgsBuilder<TOrg, TTenant> builder)
        where TOrg : class, IOrgManagementEntity
        where TTenant : class, ICurrentTenantIdentity
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HostBuilder.Services
            .AddControllers()
            .AddApplicationPart(typeof(OrgController).Assembly);

        return builder;
    }
}
