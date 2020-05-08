using Jarvis.Core.Database.Poco;
using System;
using System.Collections.Generic;

namespace Jarvis.Core.Multitenant
{
    public interface ITenantService
    {
        List<Tenant> GetHymerarchyOfTenant();

        List<Guid> GetIdHymerarchyOfTenant();
    }
}
