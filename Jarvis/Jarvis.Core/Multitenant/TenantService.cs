using Jarvis.Core.Database.Poco;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Claims;

namespace Jarvis.Core.Multitenant
{
    public class TenantService : ITenantService
    {
        public TenantService()
        {
        }

        public List<Tenant> GetHymerarchyOfTenant()
        {
            throw new NotImplementedException();
        }

        public List<Guid> GetIdHymerarchyOfTenant()
        {
            throw new NotImplementedException();
        }
    }
}