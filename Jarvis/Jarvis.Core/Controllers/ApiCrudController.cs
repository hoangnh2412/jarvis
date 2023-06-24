using System;
using System.Threading.Tasks;
using Infrastructure.Database.Abstractions;
using Infrastructure.Database.Models;
using Jarvis.Core.Abstractions;
using Jarvis.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Jarvis.Core.Controllers
{
    [ApiController]
    public class ApiCrudController<TUnitOfWork, TKey, TEntity, TModel, TCrudService, TPagingOutput, TCreateInput, TUpdateInput> 
        : ControllerBase
        where TCrudService : ICrudService<TUnitOfWork, TKey, TEntity, TModel, TPagingOutput, TCreateInput, TUpdateInput>
        where TUnitOfWork : IUnitOfWork
        where TEntity : class, IEntity<TKey>, ILogDeletedEntity, ILogDeletedVersionEntity<TKey>
    {
        [HttpGet]
        public virtual async Task<IActionResult> PaginationAsync(
            [FromQuery] Paging paging,
            [FromServices] TCrudService crudService,
            [FromServices] IDomainWorkContext workcontext)
        {
            var tenantId = workcontext.GetTenantKey();
            var userId = workcontext.GetUserKey();

            var data = await crudService.PaginationAsync(paging);
            return Ok(data);
        }

        [HttpGet("{key}")]
        public virtual async Task<IActionResult> GetAsync(
            [FromRoute] Guid key,
            [FromServices] TCrudService crudService,
            [FromServices] IDomainWorkContext workcontext)
        {
            var tenantId = workcontext.GetTenantKey();
            var userId = workcontext.GetUserKey();

            var data = await crudService.GetByKeyAsync(key);
            return Ok(data);
        }

        [HttpPost]
        public virtual async Task<IActionResult> CreateAsync(
            [FromBody] TCreateInput input,
            [FromServices] TCrudService crudService,
            [FromServices] IDomainWorkContext workcontext)
        {
            var tenantId = workcontext.GetTenantKey();
            var userId = workcontext.GetUserKey();

            var rows = await crudService.CreateAsync(input);
            if (rows == 0)
                return Conflict("Dữ liệu đã tồn tại");

            return Ok();
        }

        [HttpPut("{key}")]
        public virtual async Task<IActionResult> UpdateAsync(
            [FromRoute] Guid key,
            [FromBody] TUpdateInput input,
            [FromServices] TCrudService crudService,
            [FromServices] IDomainWorkContext workcontext)
        {
            var tenantId = workcontext.GetTenantKey();
            var userId = workcontext.GetUserKey();

            var rows = await crudService.UpdateAsync(key, input);
            if (rows == 0)
                return Conflict("Dữ liệu đã tồn tại");

            return Ok();
        }

        [HttpDelete("{key}")]
        public virtual async Task<IActionResult> DeleteAsync(
            [FromServices] TCrudService crudService,
            [FromServices] IDomainWorkContext workcontext,
            [FromRoute] Guid key,
            [FromQuery] bool isTrash = false)
        {
            var tenantId = workcontext.GetTenantKey();
            var userId = workcontext.GetUserKey();

            var rows = 0;
            if (isTrash == true)
                rows = await crudService.TrashAsync(key);
            else
                rows = await crudService.DeleteAsync(key);

            if (rows == 0)
                return NotFound("Dữ liệu không tồn tại");

            return Ok();
        }
    }
}