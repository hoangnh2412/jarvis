using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Platform.OrgStructure.Services;

namespace Platform.OrgStructure.Extensions;

public static class PlatformOrgStructureExtensions
{
    public static PlatformOrgStructureBuilder<TUnit, TLegalEntity, TJobTitle, TPosition> AddPlatformOrgStructure<
        TUnit, TLegalEntity, TJobTitle, TPosition>(this IHostApplicationBuilder builder)
        where TUnit : class, IOrganizationalUnitEntity
        where TLegalEntity : class, ILegalEntityEntity
        where TJobTitle : class, IJobTitleEntity
        where TPosition : class, IPositionEntity
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Services.Configure<OrgStructureOptions>(builder.Configuration.GetSection(OrgStructureOptions.SectionName));
        builder.Services.AddHttpContextAccessor();
        builder.Services.TryAddScoped<IOrgStructureManager, OrgStructureManager<TUnit, TLegalEntity, TJobTitle, TPosition>>();
        builder.Services.TryAddScoped<IRequestOrgUnitScope, RequestOrgUnitScopeService>();
        return new PlatformOrgStructureBuilder<TUnit, TLegalEntity, TJobTitle, TPosition>(builder);
    }
}
