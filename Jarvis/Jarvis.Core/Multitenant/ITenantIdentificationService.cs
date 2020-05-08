using Jarvis.Core.Database.Poco;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jarvis.Core.Multitenant
{
    public interface ITenantIdentificationService
    {
        Task<Tenant> GetCurrentTenantAsync(HttpContext context);

        Task<Tenant> GetParentTenantAsync(HttpContext context);

        Task<List<Tenant>> GetChildrenTenantAsync(HttpContext context);
    }
}
