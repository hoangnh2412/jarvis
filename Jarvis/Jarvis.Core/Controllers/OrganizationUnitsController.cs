using Infrastructure.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Jarvis.Core.Database;
using System;
using System.Linq;
using Jarvis.Core.Services;
using Jarvis.Core.Permissions;
using Jarvis.Core.Database.Repositories;
using Jarvis.Core.Models;
using Jarvis.Core.Database.Poco;
using System.Threading.Tasks;

namespace Jarvis.Core.Controllers
{
    [Authorize]
    [Route("organizations")]
    [ApiController]
    public class OrganizationUnitsController : ControllerBase
    {
        [HttpGet]
        [Authorize(nameof(CorePolicy.OrganizationPolicy.OrganizationUnit_Read))]
        public async Task<IActionResult> GetAsync(
            [FromQuery] Paging paging,
            [FromServices] ICoreUnitOfWork uow,
            [FromServices] IWorkContext workcontext)
        {
            var permission = await workcontext.GetContextAsync(nameof(CorePolicy.OrganizationPolicy.OrganizationUnit_Read));

            var repo = uow.GetRepository<IOrganizationUnitRepository>();
            var paged = await repo.PagingAsync(permission, paging);
            var result = new Paged<OrganizationUnitModel>
            {
                Data = paged.Data.Select(x => (OrganizationUnitModel)x),
                Page = paged.Page,
                Q = paged.Q,
                Size = paged.Size,
                TotalItems = paged.TotalItems,
                TotalPages = paged.TotalPages
            };
            return Ok(result);
        }

        [HttpGet("{code}")]
        [Authorize(nameof(CorePolicy.OrganizationPolicy.OrganizationUnit_Read))]
        public async Task<IActionResult> GetAsync(
            [FromRoute] Guid code,
            [FromServices] ICoreUnitOfWork uow,
            [FromServices] IWorkContext workcontext)
        {
            var context = await workcontext.GetContextAsync(nameof(CorePolicy.OrganizationPolicy.OrganizationUnit_Read));

            var repo = uow.GetRepository<IOrganizationUnitRepository>();
            var label = await repo.GetByCodeAsync(context, code);
            return Ok((OrganizationUnitModel)label);
        }

        [HttpPost]
        [Authorize(nameof(CorePolicy.OrganizationPolicy.OrganizationUnit_Create))]
        public async Task<IActionResult> PostAsync(
            [FromBody] OrganizationUnitModel model,
            [FromServices] ICoreUnitOfWork uow,
            [FromServices] IWorkContext workcontext)
        {
            var context = await workcontext.GetContextAsync(nameof(CorePolicy.OrganizationPolicy.OrganizationUnit_Create));

            var repo = uow.GetRepository<IOrganizationUnitRepository>();
            await repo.InsertAsync(new OrganizationUnit
            {
                Description = model.Description,
                Code = Guid.NewGuid(),
                TenantCode = await workcontext.GetTenantCodeAsync(),
                FullName = model.Name,
                CreatedAt = DateTime.Now,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = workcontext.GetUserCode()
            });
            await uow.CommitAsync();

            return Ok();
        }

        [HttpPut("{code}")]
        [Authorize(nameof(CorePolicy.OrganizationPolicy.OrganizationUnit_Update))]
        public async Task<IActionResult> PutAsync(
            [FromRoute] Guid code,
            [FromBody] OrganizationUnitModel model,
            [FromServices] ICoreUnitOfWork uow,
            [FromServices] IWorkContext workcontext)
        {
            var permission = await workcontext.GetContextAsync(nameof(CorePolicy.OrganizationPolicy.OrganizationUnit_Update));
            var repo = uow.GetRepository<IOrganizationUnitRepository>();
            var organizationUnit = await repo.GetByCodeAsync(permission, code);
            if (organizationUnit == null)
                return NotFound();

            organizationUnit.Description = model.Description;
            organizationUnit.FullName = model.Name;
            organizationUnit.UpdatedAt = DateTime.Now;
            organizationUnit.UpdatedAtUtc = DateTime.UtcNow;
            organizationUnit.UpdatedBy = Guid.Empty;
            repo.Update(organizationUnit);
            await uow.CommitAsync();

            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(nameof(CorePolicy.OrganizationPolicy.OrganizationUnit_Delete))]
        public async Task<IActionResult> DeleteAsync(
            [FromRoute] Guid code,
            [FromServices] ICoreUnitOfWork uow,
            [FromServices] IWorkContext workcontext)
        {
            var context = await workcontext.GetContextAsync(nameof(CorePolicy.OrganizationPolicy.OrganizationUnit_Delete));

            var repo = uow.GetRepository<IOrganizationUnitRepository>();
            var organizationUnit = await repo.GetByCodeAsync(context, code);
            if (organizationUnit == null)
                return NotFound();

            repo.Delete(organizationUnit);
            await uow.CommitAsync();

            return Ok();
        }
    }
}
