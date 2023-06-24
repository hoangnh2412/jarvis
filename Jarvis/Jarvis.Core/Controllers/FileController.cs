using Microsoft.AspNetCore.Mvc;
using Jarvis.Core.Database;
using Jarvis.Core.Services;
using System;
using Jarvis.Core.Database.Repositories;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Jarvis.Core.Abstractions;

namespace Jarvis.Core.Controllers
{
    [Route("file")]
    [ApiController]
    public class FileController : Controller
    {
        [Authorize]
        [HttpGet("{code}")]
        public async Task<IActionResult> GetAsync(
            [FromRoute] Guid key,
            [FromServices] IWorkContext workContext,
            [FromServices] ICoreUnitOfWork uow)
        {
            var repo = uow.GetRepository<IFileRepository>();
            var file = await repo.GetByKeyAsync(key);
            if (file == null)
                return NotFound();

            return Ok(file);
        }

        [Authorize]
        [HttpGet("download")]
        public async Task<IActionResult> DownloadAsync(
            [FromQuery] Guid key,
            [FromServices] IFileStorageService fileService)
        {
            var result = await fileService.DownloadAsync(key);

            return File(result.Content, result.FileStorage.Extension, result.FileStorage.FileName);
        }

        [Authorize]
        [HttpPost("upload")]
        public async Task<IActionResult> UploadAsync(
            [FromForm] IFormFile file,
            [FromServices] IFileStorageService fileService)
        {
            var result = await fileService.UploadAsync(file);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("view")]
        public async Task<IActionResult> ViewAsync(
            [FromQuery] Guid key,
            [FromServices] IFileStorageService fileService)
        {
            var result = await fileService.ViewAsync(key);
            return Ok(result);
        }
    }
}
