using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.File.Abstractions;
using Microsoft.AspNetCore.Hosting;

namespace Infrastructure.File
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;

        public FileService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task DeleteAsync(string bucket, string fileName)
        {
            var path = System.IO.Path.Combine(bucket, fileName);

            if (!System.IO.File.Exists(path))
                System.IO.File.Delete(path);
        }

        public async Task DeletesAsync(string bucket, List<string> fileNames)
        {
            foreach (var name in fileNames)
            {
                var path = System.IO.Path.Combine(bucket, name);

                if (!System.IO.File.Exists(name))
                    System.IO.File.Delete(name);
            }
        }

        public async Task<byte[]> DownloadAsync(string bucket, string fileName)
        {
            var path = System.IO.Path.Combine(bucket, fileName);

            return await System.IO.File.ReadAllBytesAsync(path);
        }

        public async Task UploadAsync(string bucket, string fileName, byte[] bytes)
        {
            var path = System.IO.Path.Combine(bucket, fileName);

            // if (System.IO.File.Exists(path))
            // {
            //     var extension = System.IO.Path.GetExtension(fileName);
            //     path = path.Replace($".{extension}", $"_{DateTime.Now.Ticks}.{extension}");
            // }

            await System.IO.File.WriteAllBytesAsync(path, bytes);
        }

        public Task<string> ViewAsync(string bucket, string fileName, int expireTime)
        {
            throw new NotImplementedException();
        }

        // public Task<string> GeneratePathAsync(string subPath = null, DateTime? timestamp = null)
        // {
        //     if (string.IsNullOrEmpty(subPath))
        //         return Task.FromResult(_env.WebRootPath);

        //     return Task.FromResult($"{_env.WebRootPath}-{subPath}");
        // }

        public List<string> GetFileNames(string bucket, string prefix = null)
        {
            throw new NotImplementedException();
        }
    }
}