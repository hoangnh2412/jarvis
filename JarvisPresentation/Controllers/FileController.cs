using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.File.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace JarvisPresentation.Controllers
{
    [Route("api/file")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly IWebHostEnvironment _env;

        public FileController(
            IFileService fileService,
            IWebHostEnvironment env)
        {
            _fileService = fileService;
            _env = env;
        }

        [HttpGet("download")]
        public async Task<IActionResult> DownloadAsync()
        {
            var bytes = await _fileService.DownloadAsync("vnis.pdf");
            var path = $"{_env.WebRootPath}/test.pdf";
            await System.IO.File.WriteAllBytesAsync(path, bytes);
            return Ok();
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadAsync()
        {
            var bytes = await System.IO.File.ReadAllBytesAsync($"{_env.WebRootPath}/imgs/avatar.png");
            await _fileService.UploadAsync("avatar.png", bytes);
            return Ok();
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteAsync()
        {
            await _fileService.DeleteAsync("avatar.png");
            return Ok();
        }

        [HttpGet("view")]
        public async Task<IActionResult> ViewAsync()
        {
            var url = await _fileService.GetLinkAsync("avatar.png", 60);
            return Ok();
        }
    }
}