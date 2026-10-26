using Microsoft.Extensions.DependencyInjection.Extensions;
using Platform.DDD.Domain.Repositories;
using Platform.OrgStructure;
using Platform.OrgStructure.EntityFramework.Persistence;
using Platform.OrgStructure.Extensions;
using Platform.OrgStructure.Persistence;

namespace Platform.OrgStructure.EntityFramework.Extensions;

public static class PlatformOrgStructureEntityFrameworkExtensions
{
    public static PlatformOrgStructureBuilder<TUnit, TLegalEntity, TJobTitle, TPosition> UseEntityFramework<
        TUnit, TLegalEntity, TJobTitle, TPosition, TUnitOfWork>(
        this PlatformOrgStructureBuilder<TUnit, TLegalEntity, TJobTitle, TPosition> builder)
        where TUnit : class, IOrganizationalUnitEntity
        where TLegalEntity : class, ILegalEntityEntity
        where TJobTitle : class, IJobTitleEntity
        where TPosition : class, IPositionEntity
        where TUnitOfWork : class, IUnitOfWork
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.HostBuilder.Services.TryAddScoped<
            IOrgStructureStore<TUnit, TLegalEntity, TJobTitle, TPosition>,
            OrgStructureStore<TUnit, TLegalEntity, TJobTitle, TPosition, TUnitOfWork>>();
        return builder;
    }
}
