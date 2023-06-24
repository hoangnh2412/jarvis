using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Database.Abstractions;
using Infrastructure.Database.EntityFramework;
using Infrastructure.Database.Models;
using Jarvis.Core.Database;
using Jarvis.Core.Database.Poco;
using Jarvis.Core.Models.Emails;
using Jarvis.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jarvis.Core.Controllers
{
    [Authorize]
    [Route("emails/history")]
    [ApiController]
    public class EmailHistoryController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> PaginationAsync(
            [FromQuery] Paging paging,
            [FromServices] ICoreUnitOfWork uow,
            [FromServices] IDomainWorkContext workcontext)
        {
            var tenantCode = workcontext.GetTenantKey();

            var repo = uow.GetRepository<IRepository<EmailHistory>>();
            var paged = await repo.GetQuery()
                .Where(x => x.TenantCode == tenantCode)
                .OrderByDescending(x => x.CreatedAt)
                .ToPaginationAsync(paging);

            var result = new Paged<EmailHistoryPagingOutput>
            {
                Data = paged.Data.Select(x => EmailHistoryPagingOutput.MapToModel(x)),
                Page = paged.Page,
                Q = paged.Q,
                Size = paged.Size,
                TotalItems = paged.TotalItems,
                TotalPages = paged.TotalPages
            };
            return Ok(result);
        }
    }
}