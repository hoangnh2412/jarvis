using Jarvis.Core.Database.Poco;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jarvis.Core.Multitenant
{
    public class BodyTenantService : ITenantIdentificationService
    {
        public Task<List<Tenant>> GetChildrenTenantAsync(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public Task<Tenant> GetCurrentTenantAsync(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public Task<Tenant> GetParentTenantAsync(HttpContext context)
        {
            throw new NotImplementedException();
        }
    }
}
