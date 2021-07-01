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
using System.Collections.Generic;
using Jarvis.Core.Constants;
using Jarvis.Core.Errors;

namespace Jarvis.Core.Controllers
{
    [Route("file")]
    [ApiController]
    public class FileController : Controller
    {
        private readonly ICoreUnitOfWork _uow;
        private readonly IWorkContext _workContext;
        private readonly IFileService _fileService;

        public FileController(
            ICoreUnitOfWork uow,
            IWorkContext workContext,
            IFileService fileService)
        {
            _uow = uow;
            _workContext = workContext;
            _fileService = fileService;
        }

        [Authorize]
        [HttpGet("{code}")]
        public async Task<IActionResult> GetAsync([FromRoute]Guid code)
        {
            var repo = _uow.GetRepository<IFileRepository>();
            var file = await repo.GetByIdAsync(code);
            return Ok(file);
        }

        [Authorize]
        [HttpGet("download/{isInvoice}/{id}")]
        public async Task<IActionResult> DownloadAsync([FromRoute]Guid id, bool isInvoice)
        {
            var repoFile = _uow.GetRepository<IFileRepository>();
            var file = await repoFile.GetByIdAsync(id);
            if (file == null)
                throw new Exception(FileError.KhongTimThayFile.Code.ToString());

            var bytes = await _fileService.DownloadAsync(isInvoice, id);

            return File(bytes, file.ContentType, file.Name);
        }

        /// <summary>
        /// download nhiều file
        /// chỉ cho down nhiều file pdf
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="isInvoice"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("downloads/{isInvoice}")]
        public async Task<IActionResult> DownloadsAsync([FromBody]List<Guid> ids, bool isInvoice)
        {
            var repoFile = _uow.GetRepository<IFileRepository>();

            var bytes = new List<byte[]>();

            foreach (var id in ids)
            {
                var file = await repoFile.GetByIdAsync(id);
                if (file == null)
                    throw new Exception(FileError.KhongTimThayFile.Code.ToString());

                if (file.ContentType != ContentType.Pdf)
                    throw new Exception(FileError.KhongTimPhaiFilePdf.Code.ToString());

                bytes.Add(await _fileService.DownloadAsync(isInvoice, id));
            }
            return null;
            //return File(ItextSharpCore.MergeFiles(bytes), ContentType.Pdf);
        }

        [Authorize]
        [HttpPost("upload/{isInvoice}")]
        public async Task<IActionResult> UploadAsync(IFormFile formFile, [FromRoute] bool isInvoice)
        {
            if (formFile.Length <= 0)
                throw new Exception(FileError.FileKhongCoDuLieu.Code.ToString());

            var tenantCode = await _workContext.GetTenantCodeAsync();
            var repoTenantInfo = _uow.GetRepository<ITenantRepository>();
            var tenantInfo = await repoTenantInfo.GetInfoByCodeAsync(tenantCode);

            var splited = formFile.FileName.Split('.');
            var fileNamePhys = $"{tenantInfo.TaxCode}-{Guid.NewGuid()}.{splited.Last()}";

            fileNamePhys = await _fileService.UploadAsync(isInvoice, formFile, fileNamePhys);

            //lưu file vào db

            var repoFile = _uow.GetRepository<IFileRepository>();
            var file = new Jarvis.Core.Database.Poco.File
            {
                ContentType = formFile.ContentType,
                CreatedAt = DateTime.Now,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = _workContext.GetUserCode(),
                FileName = formFile.FileName,
                Id = Guid.NewGuid(),
                Name = fileNamePhys,
                TenantCode = await _workContext.GetTenantCodeAsync(),
                Length = formFile.Length
            };

            await repoFile.InsertAsync(file);
            await _uow.CommitAsync();

            return Ok(new
            {
                file.Id,
                FileNamePhys = file.Name
            });
        }
    }
}
