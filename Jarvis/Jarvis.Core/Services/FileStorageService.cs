using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Infrastructure.Extensions;
using Infrastructure.File;
using Infrastructure.File.Abstractions;
using Jarvis.Core.Abstractions;
using Jarvis.Core.Database;
using Jarvis.Core.Database.Repositories;
using Jarvis.Core.Extensions;
using Jarvis.Core.Models.FileStorage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Jarvis.Core.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IFileService _fileService;
        private readonly ObjectStorageOption _option;
        private readonly ICoreUnitOfWork _uow;
        private readonly IDomainWorkContext _workContext;

        public FileStorageService(
            IFileService fileService,
            IOptions<ObjectStorageOption> option,
            ICoreUnitOfWork uow,
            IDomainWorkContext workContext)
        {
            _fileService = fileService;
            _option = option.Value;
            _uow = uow;
            _workContext = workContext;
        }

        public virtual async Task<FileDownloadOutput> DownloadAsync(Guid key)
        {
            var repo = _uow.GetRepository<IFileRepository>();
            var file = await repo.GetByKeyAsync(key);
            if (file == null)
                throw new Exception($"Không tìm thấy file: {key}");

            return new FileDownloadOutput
            {
                BucketName = file.BucketName,
                Content = await _fileService.DownloadAsync(file.BucketName, file.Path),
                FileName = file.FileName,
                FileStorage = file,
                Path = file.Path
            };
        }

        public virtual async Task<FileUploadOutput> UploadAsync(IFormFile file, string subPath = null)
        {
            if (file.Length <= 0)
                throw new Exception("File không có dữ liệu");

            var bytes = await file.ReadToBytesAsync();
            return await UploadAsync(bytes, file.FileName, file.ContentType, file.Length, subPath);
        }

        public virtual async Task<FileUploadOutput> UploadAsync(byte[] bytes, string fileName, string contentType, long length, string subPath = null)
        {
            var now = DateTime.UtcNow;
            var path = System.IO.Path.Combine(_workContext.GetTenantKey().ToString(), now.Day.ToString("00"), fileName);
            if (!string.IsNullOrEmpty(subPath))
                path = System.IO.Path.Combine(subPath, path);

            var bucket = $"{_option.BucketName}-{now.ToString("yyyyMM")}";
            await _fileService.UploadAsync(bucket, path, bytes);

            var hash = FormatExtension.ByteArrayToHex(CryptographyExtension.GetHash(HashAlgorithmName.SHA256, bytes));

            var key = Guid.NewGuid();
            var repo = _uow.GetRepository<IFileRepository>();
            await repo.InsertAsync(new Jarvis.Core.Database.Poco.File
            {
                Key = key,
                CreatedAt = DateTime.UtcNow,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = _workContext.GetUserKey(),
                FileName = fileName,
                TenantCode = _workContext.GetTenantKey(),
                Extension = contentType,
                Length = length,
                Path = path,
                BucketName = bucket,
                Metadata = hash
            });
            await _uow.CommitAsync();

            return new FileUploadOutput
            {
                Key = key,
                FileName = fileName,
                Path = path,
                BucketName = bucket,
                Metadata = hash
            };
        }

        public async Task<string> ViewAsync(Guid key)
        {
            var repo = _uow.GetRepository<IFileRepository>();
            var file = await repo.GetByKeyAsync(key);
            if (file == null)
                throw new Exception($"Không tìm thấy file: {key}");

            return $"{file.BucketName}/{file.Path}";
        }
    }
}