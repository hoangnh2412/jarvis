using System.Threading.Tasks;
using Infrastructure.Database.Abstractions;
using Jarvis.Core.Abstractions;
using Jarvis.Core.Extensions;
using Jarvis.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jarvis.Core.Controllers
{
    [ApiController]
    public class ApiCrudieController<TUnitOfWork, TKey, TEntity, TModel, TCrudService, TPagingOutput, TCreateInput, TUpdateInput, TImportService, TImportInput>
        : ApiCrudController<TUnitOfWork, TKey, TEntity, TModel, TCrudService, TPagingOutput, TCreateInput, TUpdateInput>
        where TCrudService : ICrudService<TUnitOfWork, TKey, TEntity, TModel, TPagingOutput, TCreateInput, TUpdateInput>
        where TImportService : IImportService<TUnitOfWork, TKey, TEntity, TModel, TImportInput>
        where TUnitOfWork : IUnitOfWork
        where TEntity : class, IEntity<TKey>, ILogDeletedEntity, ILogDeletedVersionEntity<TKey>
    {
        [HttpPost("import")]
        public virtual async Task<IActionResult> ImportAsync(
            [FromForm] IFormFile file,
            [FromServices] IDomainWorkContext workContext,
            [FromServices] TImportService importService
        )
        {
            var tenantCode = workContext.GetTenantKey();
            var userCode = workContext.GetUserKey();

            var bytes = await FormExtension.ReadToBytesAsync(file);
            var result = await importService.ImportAsync(bytes);
            if (result > 0)
                return Ok();

            return StatusCode(500, "Lỗi trong quá trình import");
        }
    }
}