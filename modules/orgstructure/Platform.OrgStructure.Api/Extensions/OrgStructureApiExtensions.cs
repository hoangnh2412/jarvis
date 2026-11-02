using Microsoft.Extensions.DependencyInjection;
using Platform.OrgStructure.Api.Controllers;

namespace Platform.OrgStructure.Api.Extensions;

public static class OrgStructureApiExtensions
{
    /// <summary>Gắn controller <c>api/v1/org-structure</c>; service danh mục đăng ký qua <c>AddOrgStructureCatalogs</c> phía EntityFramework.</summary>
    public static IServiceCollection AddOrgStructureApi(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services
            .AddControllers()
            .AddApplicationPart(typeof(OrgStructureController).Assembly);

        return services;
    }
}
