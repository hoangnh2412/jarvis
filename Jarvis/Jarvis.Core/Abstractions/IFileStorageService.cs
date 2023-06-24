using System;
using System.Threading.Tasks;
using Jarvis.Core.Models.FileStorage;
using Microsoft.AspNetCore.Http;

namespace Jarvis.Core.Abstractions
{
    public interface IFileStorageService
    {
        Task<FileDownloadOutput> DownloadAsync(Guid key);

        Task<FileUploadOutput> UploadAsync(IFormFile file, string subPath = null);

        Task<FileUploadOutput> UploadAsync(byte[] bytes, string fileName, string contentType, long length, string subPath = null);

        Task<string> ViewAsync(Guid key);
    }
}