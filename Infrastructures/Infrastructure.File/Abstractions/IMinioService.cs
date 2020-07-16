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
        Task UploadAsync(string fileName, byte[] bytes);

        /// <summary>
        /// Tải xuống file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        Task<byte[]> DownloadAsync(string fileName);

        /// <summary>
        /// Xoá 1 file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        Task DeleteAsync(string fileName);

        /// <summary>
        /// Xoá nhiều file
        /// </summary>
        /// <param name="fileNames"></param>
        /// <returns></returns>
        Task DeletesAsync(List<string> fileNames);

        /// <summary>
        /// Lấy link xem file
        /// </summary>
        /// <param name="fileName">Tên file</param>
        /// <param name="expireTime">Thời gian link tồn tại (giây)</param>
        /// <returns></returns>
        Task<string> GetLinkAsync(string fileName, int expireTime);
    }
}