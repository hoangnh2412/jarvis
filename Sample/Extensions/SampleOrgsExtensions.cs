using Platform.MultiOrg;
using Platform.Multitenancy;
using Platform.Orgs.Api.Extensions;
using Platform.Orgs.EntityFramework.Extensions;
using Platform.Orgs.Extensions;
using Platform.Orgs.Services;
using Sample.Organization;
using Sample.Persistence;

namespace Sample.Extensions;

/// <summary>Đăng ký catalog Org. Cột <c>TaxCode</c> map trong <c>TenantDbContext</c>.</summary>
public static class SampleOrgsExtensions
{
    public static WebApplicationBuilder AddSampleOrgs(this WebApplicationBuilder builder)
    {
        builder.AddCurrentOrg<OrgCatalogOrgProfile>();

        builder.AddPlatformOrgs<Sample.Entities.Org, CurrentTenantInfo>()
            .UseEntityFramework<Sample.Entities.Org, CurrentTenantInfo, ITenantUnitOfWork>()
            .UseHooks<SampleOrgCatalogHooks>()
            .UseReadModel<SampleOrgDetail, SampleOrgReadMapper>()
            .UseAsCurrentOrgStore()
            .UseHttpApi();

        return builder;
    }
}
