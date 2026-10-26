using Microsoft.Extensions.Hosting;

namespace Platform.OrgStructure.Extensions;

public sealed class PlatformOrgStructureBuilder<TUnit, TLegalEntity, TJobTitle, TPosition>
    where TUnit : class, IOrganizationalUnitEntity
    where TLegalEntity : class, ILegalEntityEntity
    where TJobTitle : class, IJobTitleEntity
    where TPosition : class, IPositionEntity
{
    internal PlatformOrgStructureBuilder(IHostApplicationBuilder hostBuilder)
    {
        HostBuilder = hostBuilder;
    }

    public IHostApplicationBuilder HostBuilder { get; }
}
