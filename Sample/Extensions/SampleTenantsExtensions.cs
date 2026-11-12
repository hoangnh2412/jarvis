using Platform.Tenants.EntityFramework.Extensions;
using Platform.Tenants.Extensions;
using Sample.Entities;
using Sample.Persistence;

namespace Sample.Extensions;

/// <summary>Catalog tenant trên Master để <c>POST /api/v1/app/account/login</c> tra theo mã.</summary>
public static class SampleTenantsExtensions
{
    public static WebApplicationBuilder AddSampleTenants(this WebApplicationBuilder builder)
    {
        builder.AddPlatformTenants<Tenant>()
            .UseEntityFramework<Tenant, ISampleMasterUnitOfWork>();
        return builder;
    }
}
