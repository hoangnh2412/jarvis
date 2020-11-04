// using Infrastructure.Database.Models;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Jarvis.Core.Database;
// using System;
// using System.Linq;
// using Jarvis.Core.Services;
// using Jarvis.Core.Permissions;
// using Jarvis.Core.Database.Repositories;
// using Jarvis.Core.Models;
// using Jarvis.Core.Database.Poco;
// using System.Threading.Tasks;

// namespace Jarvis.Core.Controllers
// {
//     [Authorize]
//     [Route("organizations")]
//     [ApiController]
//     public class OrganizationUnitsController : ControllerBase
//     {
//         private readonly ICoreUnitOfWork _uow;
//         private readonly IWorkContext _workcontext;

//         public OrganizationUnitsController(
//             ICoreUnitOfWork uow,
//             IWorkContext workcontext)
//         {
//             _uow = uow;
//             _workcontext = workcontext;
//         }

//         [HttpGet]
//         [Authorize(nameof(CorePolicy.OrganizationPolicy.Organization_Read))]
//         public async Task<IActionResult> GetAsync([FromQuery]Paging paging)
//         {
//             var permission = await _workcontext.GetContextAsync(nameof(CorePolicy.OrganizationPolicy.Organization_Read));

//             var repo = _uow.GetRepository<IOrganizationUnitRepository>();
//             var paged = await repo.PagingAsync(permission, paging);
//             var result = new Paged<OrganizationUnitModel>
//             {
//                 Data = paged.Data.Select(x => (OrganizationUnitModel)x),
//                 Page = paged.Page,
//                 Q = paged.Q,
//                 Size = paged.Size,
//                 TotalItems = paged.TotalItems,
//                 TotalPages = paged.TotalPages
//             };
//             return Ok(result);
//         }

//         [HttpGet("{code}")]
//         [Authorize(nameof(CorePolicy.OrganizationPolicy.Organization_Read))]
//         public async Task<IActionResult> GetAsync(Guid code)
//         {
//             var context = await _workcontext.GetContextAsync(nameof(CorePolicy.OrganizationPolicy.Organization_Read));

//             var repo = _uow.GetRepository<IOrganizationUnitRepository>();
//             var label = await repo.GetByCodeAsync(context, code);
//             return Ok((OrganizationUnitModel)label);
//         }

//         [HttpPost]
//         [Authorize(nameof(CorePolicy.OrganizationPolicy.Organization_Create))]
//         public async Task<IActionResult> PostAsync([FromBody] OrganizationUnitModel model)
//         {
//             var context = await _workcontext.GetContextAsync(nameof(CorePolicy.OrganizationPolicy.Organization_Create));

//             var repo = _uow.GetRepository<IOrganizationUnitRepository>();
//             await repo.InsertAsync(new OrganizationUnit
//             {
//                 Description = model.Description,
//                 Code = Guid.NewGuid(),
//                 TenantCode = await _workcontext.GetTenantCodeAsync(),
//                 Name = model.Name,
//                 FullName = model.FullName,
//                 CreatedAt = DateTime.Now,
//                 CreatedAtUtc = DateTime.UtcNow,
//                 CreatedBy = _workcontext.GetUserCode()
//             });
//             await _uow.CommitAsync();

//             return Ok();
//         }

//         [HttpPut("{code}")]
//         [Authorize(nameof(CorePolicy.OrganizationPolicy.Organization_Update))]
//         public async Task<IActionResult> PutAsync([FromRoute]Guid code, [FromBody]OrganizationUnitModel model)
//         {
//             var permission = await _workcontext.GetContextAsync(nameof(CorePolicy.OrganizationPolicy.Organization_Update));
//             var repo = _uow.GetRepository<IOrganizationUnitRepository>();
//             var organizationUnit = await repo.GetByCodeAsync(permission, code);
//             if (organizationUnit == null)
//                 return NotFound();

//             organizationUnit.Description = model.Description;
//             organizationUnit.FullName = model.FullName;
//             organizationUnit.UpdatedAt = DateTime.Now;
//             organizationUnit.UpdatedAtUtc = DateTime.UtcNow;
//             organizationUnit.UpdatedBy = Guid.Empty;
//             repo.Update(organizationUnit);
//             await _uow.CommitAsync();

//             return Ok();
//         }

//         [HttpDelete("{id}")]
//         [Authorize(nameof(CorePolicy.OrganizationPolicy.Organization_Delete))]
//         public async Task<IActionResult> DeleteAsync([FromRoute]Guid code)
//         {
//             var context = await _workcontext.GetContextAsync(nameof(CorePolicy.OrganizationPolicy.Organization_Delete));

//             var repo = _uow.GetRepository<IOrganizationUnitRepository>();
//             var organizationUnit = await repo.GetByCodeAsync(context, code);
//             if (organizationUnit == null)
//                 return NotFound();

//             repo.Delete(organizationUnit);
//             await _uow.CommitAsync();

//             return Ok();
//         }
//     }
// }
