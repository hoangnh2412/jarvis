using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Jarvis.Core.Abstractions
{
    public interface IFileService
    {
        /// <summary>
        /// upload file
        /// </summary>
        /// <param name="isInvoice"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        Task<byte[]> DownloadAsync(Guid code, bool isInvoice = false);

        /// <summary>
        /// download file
        /// </summary>
        /// <param name="isInvoice"></param>
        /// <param name="formFile"></param>
        /// <param name="fileNamePhys"></param>
        /// <returns></returns>
        Task<string> UploadAsync(IFormFile formFile, string fileNamePhys, bool isInvoice = false);


        /// <summary>
        /// upload file 
        /// </summary>
        /// <param name="isInvoice"></param>
        /// <param name="bytes"></param>
        /// <param name="fileNamePhys"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        Task<string> UploadAsync(byte[] bytes, string fileNamePhys, string fileName, bool isInvoice = false);


        /// <summary>
        /// lấy đường dẫn thư mục
        /// </summary>
        /// <param name="isDefault"></param>
        /// <param name="CreatedAt"></param>
        /// <returns></returns>
        Task<string> GetFilePath(DateTime CreatedAt, bool isDefault = false);

        /// <summary>
        /// lưu file xml hóa đơn đã ký
        /// </summary>
        /// <param name="CreatedBy"></param>
        /// <param name="tenantCode"></param>
        /// <param name="xml"></param>
        /// <param name="isDefault"></param>
        /// <param name="sellerTaxCode"></param>
        /// <param name="templateNo"></param>
        /// <param name="serialNo"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        Task<Guid> SaveFileXmlAsync(Guid CreatedBy, Guid tenantCode, string xml, string sellerTaxCode, string templateNo, string serialNo, int number, bool isDefault = false);
    }
}
