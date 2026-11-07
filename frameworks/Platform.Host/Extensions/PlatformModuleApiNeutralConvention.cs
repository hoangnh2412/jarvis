using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Platform.Host.Extensions;

/// <summary>
/// Controller identity và org-structure của Platform không gắn <c>[ApiVersion]</c>.
/// Host bật versioning thì đánh các controller đó là version-neutral để route <c>/api/v1/...</c> khớp.
/// </summary>
public sealed class PlatformModuleApiNeutralConvention : IControllerConvention
{
    public bool Apply(IControllerConventionBuilder builder, ControllerModel controller)
    {
        var ns = controller.ControllerType.Namespace;
        if (ns is null)
            return false;

        var isPlatformModule =
            ns.StartsWith("Platform.Modules.IdentityAdmin.Api", StringComparison.Ordinal)
            || ns.StartsWith("Platform.Modules.Account.Api", StringComparison.Ordinal)
            || ns.StartsWith("Platform.OrgStructure.Api", StringComparison.Ordinal);
        if (!isPlatformModule)
            return false;

        builder.IsApiVersionNeutral();
        return true;
    }
}
