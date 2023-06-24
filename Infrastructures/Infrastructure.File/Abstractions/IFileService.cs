using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.File.Abstractions
{
    public interface IFileService
    {
        /// <summary>
        /// Tải lên file
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        Task UploadAsync(string bucket, string fileName, byte[] bytes);

        /// <summary>
        /// Tải xuống file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        Task<byte[]> DownloadAsync(string bucket, string fileName);

        /// <summary>
        /// Xoá 1 file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        Task DeleteAsync(string bucket, string fileName);

        /// <summary>
        /// Xoá nhiều file
        /// </summary>
        /// <param name="fileNames"></param>
        /// <returns></returns>
        Task DeletesAsync(string bucket, List<string> fileNames);

        /// <summary>
        /// Lấy link xem file
        /// </summary>
        /// <param name="fileName">Tên file</param>
        /// <param name="expireTime">Thời gian link tồn tại (giây)</param>
        /// <returns></returns>
        Task<string> ViewAsync(string bucket, string fileName, int expireTime = 1800);

        /// <summary>
        /// Lấy danh sách file
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        List<string> GetFileNames(string bucket, string prefix = null);
    }
}