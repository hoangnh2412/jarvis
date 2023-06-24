using Infrastructure.Database.Abstractions;
using System;

namespace Jarvis.Core.Database.Poco
{
    public class File : IEntity<int>, ITenantEntity, ILogCreatedEntity, IPermissionEntity
    {
        public int Id { get; set; }
        public Guid Key { get; set; }
        public Guid TenantCode { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        /// <summary>
        /// Tên file hiển thị
        /// </summary>
        /// <value></value>
        public string Path { get; set; }

        /// <summary>
        /// Đuôi file. Ví dụ jpg
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// Tên file và cả đuôi file. Ví dụ image.jpg
        /// </summary>
        public string FileName { get; set; }

        public string BucketName { get; set; }

        /// <summary>
        /// Dung lượng file
        /// </summary>
        public long Length { get; set; }

        public string Metadata { get; set; }
    }
}
