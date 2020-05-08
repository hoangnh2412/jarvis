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
        Task<byte[]> DownloadAsync(bool isInvoice, Guid code);

        /// <summary>
        /// download file
        /// </summary>
        /// <param name="isInvoice"></param>
        /// <param name="formFile"></param>
        /// <param name="fileNamePhys"></param>
        /// <returns></returns>
        Task<string> UploadAsync(bool isInvoice, IFormFile formFile, string fileNamePhys);


        /// <summary>
        /// upload file 
        /// </summary>
        /// <param name="isInvoice"></param>
        /// <param name="bytes"></param>
        /// <param name="fileNamePhys"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        Task<string> UploadAsync(bool isInvoice, byte[] bytes, string fileNamePhys, string fileName);


        /// <summary>
        /// lấy đường dẫn thư mục
        /// </summary>
        /// <param name="isDefault"></param>
        /// <param name="CreatedAt"></param>
        /// <returns></returns>
        Task<string> GetFilePath(bool isDefault, DateTime CreatedAt);

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
        Task<Guid> SaveFileXmlAsync(Guid CreatedBy, Guid tenantCode, string xml, bool isDefault, string sellerTaxCode, string templateNo, string serialNo, int number);


    }
}
