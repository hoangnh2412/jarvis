using Jarvis.Core.Abstractions;
using Jarvis.Core.Constants;
using Jarvis.Core.Database;
using Jarvis.Core.Database.Repositories;
using Jarvis.Core.Errors;
using Jarvis.Core.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Jarvis.Core.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ICoreUnitOfWork _uowCore;
        private readonly FileUploadOption _options;

        public FileService(
            IWebHostEnvironment env,
            ICoreUnitOfWork uowCore,
            IOptions<FileUploadOption> options)
        {
            _env = env;
            _uowCore = uowCore;
            _options = options.Value;
        }


        public async Task<byte[]> DownloadAsync(bool isDefault, Guid id)
        {
            ////lấy tên file từ db
            var repoFile = _uowCore.GetRepository<IFileRepository>();
            var file = await repoFile.GetByIdAsync(id);
            if (file == null)
                throw new Exception(FileError.KhongTimThayFile.Code.ToString());

            //lấy tên thư mục lưu file
            var folderPath = await GetFilePath(isDefault, file.CreatedAt);

            var filePath = Path.Combine(folderPath, file.Name);

            if (!System.IO.File.Exists(filePath))
                throw new Exception(FileError.KhongTimThayFileVatLy.Code.ToString());

            var bytes = System.IO.File.ReadAllBytes(filePath);
            return bytes;
        }


        public async Task<string> UploadAsync(bool isInvoice, IFormFile formFile, string fileNamePhys)
        {
            var folderPath = await GetFilePath(isInvoice, DateTime.UtcNow);
            var filePath = Path.Combine(folderPath, fileNamePhys);

            //Nếu đã có file trùng tên thì đổi tên file hiện tại
            if (File.Exists(filePath))
            {
                var matches = Regex.Matches(fileNamePhys, @"\((\d+)\)");
                if (matches.Count == 0) //Trùng lần đầu tiên
                {
                    //Thêm (x) ở đuôi file
                    var splited = fileNamePhys.Split('.');
                    var extension = splited[splited.Length - 1];
                    fileNamePhys = $"{string.Join(".", splited.Take(splited.Length - 1))}_(1).{extension}";
                }
                else //Đã bị đổi tên
                {
                    //Match cuối là thứ tự file bị trùng
                    var match = matches[matches.Count - 1];

                    //Lấy số thứ tự
                    var str = match.Value.Substring(1, match.Value.Length - 2);
                    var index = int.Parse(str);
                    index++;
                    fileNamePhys = fileNamePhys.Remove(match.Index, match.Length).Insert(match.Index, $"({index})");
                }
            }

            //Upload file với tên mới
            filePath = Path.Combine(folderPath, fileNamePhys);
            //File.WriteAllBytes(filePath, bytes);

            if (formFile.Length > 0)
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await formFile.CopyToAsync(stream);
                }
            }

            return fileNamePhys;
        }

        public async Task<string> UploadAsync(bool isInvoice, byte[] bytes, string fileNamePhys, string fileName)
        {
            var folderPath = await GetFilePath(isInvoice, DateTime.UtcNow);
            var filePath = Path.Combine(folderPath, fileNamePhys);

            //Nếu đã có file trùng tên thì đổi tên file hiện tại
            if (File.Exists(filePath))
            {
                var matches = Regex.Matches(fileNamePhys, @"\((\d+)\)");
                if (matches.Count == 0) //Trùng lần đầu tiên
                {
                    //Thêm (x) ở đuôi file
                    var splited = fileNamePhys.Split('.');
                    var extension = splited[splited.Length - 1];
                    fileNamePhys = $"{string.Join(".", splited.Take(splited.Length - 1))}_(1).{extension}";
                }
                else //Đã bị đổi tên
                {
                    //Match cuối là thứ tự file bị trùng
                    var match = matches[matches.Count - 1];

                    //Lấy số thứ tự
                    var str = match.Value.Substring(1, match.Value.Length - 2);
                    var index = int.Parse(str);
                    index++;
                    fileNamePhys = fileNamePhys.Remove(match.Index, match.Length).Insert(match.Index, $"({index})");
                }
            }

            //Upload file với tên mới
            filePath = Path.Combine(folderPath, fileNamePhys);

            File.WriteAllBytes(filePath, bytes);

            return fileNamePhys;
        }

        /// <summary>
        /// lấy 
        /// </summary>
        /// <param name="isDefault"></param>
        /// <param name="CreatedAt"></param>
        /// <returns></returns>
        public async Task<string> GetFilePath(bool isInvoice, DateTime CreatedAt)
        {
            var folderName = _options.Default; //thư mục mặc định

            //nếu là hóa đơn thì sẽ lấy ở thư mục trong cấu hình setting
            if (!isInvoice)
            {
                var repoSetting = _uowCore.GetRepository<ISettingRepository>();
                var setting = await repoSetting.GetByKeyAsync(Guid.Empty, SettingKey.FilePathOption.ToString());
                if (setting != null && string.IsNullOrEmpty(setting.Value))
                {
                    folderName = setting.Value;
                }
            }

            var path = Path.Combine(_env.WebRootPath, folderName, CreatedAt.Year.ToString(), CreatedAt.Month.ToString(), CreatedAt.Day.ToString());
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }

        public async Task<Guid> SaveFileXmlAsync(Guid CreatedBy, Guid tenantCode, string xml, bool isDefault, string sellerTaxCode, string templateNo, string serialNo, int number)
        {
            var folderPath = await GetFilePath(isDefault, DateTime.UtcNow);
            var fileNamePhys = $"{sellerTaxCode}_{templateNo}_{serialNo}_{number}_{DateTime.Now.Ticks}.xml".Replace("/", "-");
            var filePath = Path.Combine(folderPath, fileNamePhys);

            //Nếu đã có file trùng tên thì đổi tên file hiện tại
            if (File.Exists(filePath))
            {
                var matches = Regex.Matches(fileNamePhys, @"\((\d+)\)");
                if (matches.Count == 0) //Trùng lần đầu tiên
                {
                    //Thêm (x) ở đuôi file
                    var splited = fileNamePhys.Split('.');
                    var extension = splited[splited.Length - 1];
                    fileNamePhys = $"{string.Join(".", splited.Take(splited.Length - 1))}_(1).{extension}";
                }
                else //Đã bị đổi tên
                {
                    //Match cuối là thứ tự file bị trùng
                    var match = matches[matches.Count - 1];

                    //Lấy số thứ tự
                    var str = match.Value.Substring(1, match.Value.Length - 2);
                    var index = int.Parse(str);
                    index++;
                    fileNamePhys = fileNamePhys.Remove(match.Index, match.Length).Insert(match.Index, $"({index})");
                }
            }

            //Upload file với tên mới
            filePath = Path.Combine(folderPath, fileNamePhys);

            var bytes = Encoding.UTF8.GetBytes(xml);
            if (bytes.Length > 0)
            {
                File.WriteAllBytes(filePath, bytes);
            }

            //lưu vào db 
            var repoFile = _uowCore.GetRepository<IFileRepository>();
            var idFile = Guid.NewGuid();

            await repoFile.InsertAsync(new Database.Poco.File
            {
                ContentType = ContentType.Xml,
                CreatedAt = DateTime.Now,
                CreatedAtUtc = DateTime.UtcNow,
                FileName = fileNamePhys,
                Id = idFile,
                Length = bytes.Length,
                Name = fileNamePhys,
                TenantCode = tenantCode,
                CreatedBy = CreatedBy
            });

            await _uowCore.CommitAsync();

            return idFile;
        }

    }
}
