using Microsoft.AspNetCore.Mvc;
using Jarvis.Core.Database;
using Jarvis.Core.Services;
using System;
using System.Linq;
using Jarvis.Core.Database.Repositories;
using System.Threading.Tasks;
using Jarvis.Core.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace Jarvis.Core.Controllers
{
    [Route("file")]
    [ApiController]
    public class FileController : Controller
    {
        [Authorize]
        [HttpGet("{code}")]
        public async Task<IActionResult> GetAsync(
            [FromRoute] Guid code,
            [FromServices] IWorkContext workContext,
            [FromServices] ICoreUnitOfWork uow)
        {
            var idTenant = await workContext.GetTenantCodeAsync();

            var repo = uow.GetRepository<IFileRepository>();
            var file = await repo.GetByIdAsync(code);
            return Ok(file);
        }

        [Authorize]
        [HttpGet("download")]
        public async Task<IActionResult> DownloadAsync(
            [FromQuery] Guid id,
            [FromServices] ICoreUnitOfWork uow,
            [FromServices] IFileService fileService)
        {
            var repoFile = uow.GetRepository<IFileRepository>();
            var file = await repoFile.GetByIdAsync(id);
            if (file == null)
                throw new Exception("Không tìm thấy file");

            var bytes = await fileService.DownloadAsync(id);

            return File(bytes, file.ContentType, file.Name);
        }

        [Authorize]
        [HttpPost("upload")]
        public async Task<IActionResult> UploadAsync(
            [FromForm] IFormFile formFile,
            [FromServices] IWorkContext workContext,
            [FromServices] ICoreUnitOfWork uow,
            [FromServices] IFileService fileService)
        {
            if (formFile.Length <= 0)
                throw new Exception("File không có dữ liệu");

            var tenantCode = await workContext.GetTenantCodeAsync();
            var repoTenantInfo = uow.GetRepository<ITenantRepository>();
            var tenantInfo = await repoTenantInfo.GetInfoByCodeAsync(tenantCode);

            var splited = formFile.FileName.Split('.');
            var fileNamePhys = $"{tenantInfo.TaxCode}-{Guid.NewGuid()}.{splited.Last()}";

            fileNamePhys = await fileService.UploadAsync(formFile, fileNamePhys);

            //lưu file vào db

            var repoFile = uow.GetRepository<IFileRepository>();
            var file = new Jarvis.Core.Database.Poco.File
            {
                ContentType = formFile.ContentType,
                CreatedAt = DateTime.Now,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = workContext.GetUserCode(),
                FileName = formFile.FileName,
                Id = Guid.NewGuid(),
                Name = fileNamePhys,
                TenantCode = await workContext.GetTenantCodeAsync(),
                Length = formFile.Length
            };

            await repoFile.InsertAsync(file);
            await uow.CommitAsync();

            return Ok(new
            {
                file.Id,
                FileNamePhys = file.Name
            });
        }
    }
}
